import test from 'node:test';
import assert from 'node:assert/strict';
import {studioAddresses, isStudioHost} from '../server/network-access.mjs';

test('LAN host whitelist uses local private interfaces and the actual listening port', () => {
  const addresses = studioAddresses({ethernet:[
    {family:'IPv4',address:'192.168.72.39'},
    {family:'IPv4',address:'10.1.2.3'},
    {family:'IPv4',address:'172.16.0.2'},
    {family:'IPv4',address:'172.32.0.2'},
    {family:'IPv4',address:'203.0.113.1'},
    {family:'IPv6',address:'::1'}
  ]});
  for (const host of ['127.0.0.1','localhost','192.168.72.39','10.1.2.3','172.16.0.2']) assert.ok(isStudioHost(`${host}:18091`,18091,addresses));
  for (const host of ['192.168.72.40:18091','192.168.72.39:80','172.32.0.2:18091','203.0.113.1:18091','evil.example:18091','192.168.72.39.evil.example:18091','user@192.168.72.39:18091',undefined]) assert.equal(isStudioHost(host,18091,addresses),false);
});
