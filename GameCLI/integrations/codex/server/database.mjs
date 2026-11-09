import {readFile} from 'node:fs/promises';
import {parseEnv} from 'node:util';
import {createServer} from 'node:net';
import pg from 'pg';
import ssh2 from 'ssh2';

export async function databaseConfig() {
  let local={};
  try {local=parseEnv(await readFile(new URL('../.env.database',import.meta.url),'utf8'));}
  catch(e){if(e.code!=='ENOENT')throw e;}
  return {...local,...process.env};
}
export async function openDatabase(config) {
  config??=await databaseConfig();
  if(!config.PGDATABASE) throw new Error('尚未配置模板数据库。');
  let ssh, tunnel, port=Number(config.PGPORT||5432), host=config.PGHOST||'127.0.0.1';
  const sockets=new Set();
  if(config.SSH_HOST) {
    if(!/^[a-f0-9]{64}$/i.test(config.SSH_HOST_SHA256??'')) throw new Error('SSH 服务器指纹未配置。');
    ssh=new ssh2.Client();
    await new Promise((resolve,reject)=>{
      ssh.once('ready',resolve);ssh.once('error',reject);
      ssh.connect({host:config.SSH_HOST,port:Number(config.SSH_PORT||22),username:config.SSH_USER,password:config.SSH_PASSWORD,hostHash:'sha256',hostVerifier:hash=>hash===config.SSH_HOST_SHA256,readyTimeout:10000,keepaliveInterval:10000,keepaliveCountMax:3});
    });
    ssh.on('error',()=>{for(const socket of sockets)socket.destroy();});
    tunnel=createServer(socket=>{
      sockets.add(socket);socket.on('close',()=>sockets.delete(socket));socket.on('error',()=>{});
      ssh.forwardOut('127.0.0.1',socket.remotePort,host,port,(error,stream)=>{
        if(error){socket.destroy();return;}
        stream.on('error',()=>socket.destroy());socket.on('close',()=>stream.destroy());socket.pipe(stream).pipe(socket);
      });
    });
    await new Promise((resolve,reject)=>{tunnel.once('error',reject);tunnel.listen(0,'127.0.0.1',resolve);});
  }
  const pool=new pg.Pool({host:tunnel?'127.0.0.1':host,port:tunnel?tunnel.address().port:port,database:config.PGDATABASE,user:config.PGUSER,password:config.PGPASSWORD,max:4,connectionTimeoutMillis:10000,idleTimeoutMillis:10000,statement_timeout:15000});
  pool.on('error',()=>{});
  return {pool,async close(){await pool.end();for(const socket of sockets)socket.destroy();if(tunnel)await new Promise(resolve=>tunnel.close(resolve));ssh?.end();}};
}
