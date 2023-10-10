import http from 'http';
import httpProxy from 'http-proxy';

const sourcePort = 8000;
const sourceAddress = 'localhost';
const targetPort = 443;
const targetAddress = 'kafe.fi.muni.cz';

const target = {
    protocol: 'https:',
    host: targetAddress,
    port: targetPort,
};

var proxy = httpProxy.createProxyServer({
    target,
    changeOrigin: true,
    cookieDomainRewrite: {
        [targetAddress]: sourceAddress,
    },
});

proxy.on('proxyRes', (proxyRes, req, res) => {
    res.setHeader('X-Proxy', targetAddress);
    res.setHeader('Access-Control-Allow-Origin', req.headers.origin || '*');
    res.setHeader('Access-Control-Allow-Credentials', 'true');

    if (req.method === 'OPTIONS') {
        res.setHeader('Access-Control-Allow-Methods', 'HEAD, GET, POST, PUT, PATCH, DELETE');
        res.setHeader('Access-Control-Allow-Headers', 'Origin, Content-Type');

        res.writeHead(200, res.getHeaders());
    }

    console.log(`${sourceAddress} [<- ] ${targetAddress}    ${req.method} ${req.url} ${res.statusCode}`);
});

proxy.on('proxyReq', (proxyReq, req) => {
    console.log(`${sourceAddress} [ ->] ${targetAddress}    ${req.method} ${req.url}`);
});

var server = http.createServer((req, res) => {
    proxy.web(req, res);
});

console.log('Proxy running');
console.log(`${sourceAddress}:${sourcePort} <-> ${targetAddress}:${targetPort}`);
console.log('');
server.listen(sourcePort);
