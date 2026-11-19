import { networkInterfaces } from 'node:os';

export function studioAddresses(interfaces = networkInterfaces()) {
  const addresses = new Set(['127.0.0.1', 'localhost']);
  for (const entries of Object.values(interfaces)) for (const entry of entries ?? []) {
    if (entry.family !== 'IPv4') continue;
    const [a, b] = entry.address.split('.').map(Number);
    if (a === 10 || (a === 172 && b >= 16 && b <= 31) || (a === 192 && b === 168)) addresses.add(entry.address);
  }
  return addresses;
}

export function isStudioHost(host, port, addresses = studioAddresses()) {
  // Accept only exact local authorities, not arbitrary Host headers or forwarded hosts.
  return [...addresses].some(address => host === `${address}:${port}`);
}
