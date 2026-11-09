import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { createMcpServer } from './mcp.mjs';
const server = createMcpServer();
await server.connect(new StdioServerTransport());
process.stdin.once('end', () => void server.close());
process.once('SIGINT', () => void server.close());
process.once('SIGTERM', () => void server.close());
