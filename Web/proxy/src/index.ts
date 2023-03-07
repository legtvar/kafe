import http from 'http';
import httpProxy from 'http-proxy';

const target = {
    protocol: 'https:',
    host: 'kafe.fi.muni.cz',
    port: 443,
};

var proxy = httpProxy.createProxyServer({
    target,
    changeOrigin: true,
    cookieDomainRewrite: {
        'kafe.fi.muni.cz': 'localhost',
    },
});

proxy.on('proxyRes', (proxyRes, req, res) => {
    res.setHeader('X-Proxy', target.host);
    res.setHeader('Access-Control-Allow-Origin', req.headers.origin || '*');
    res.setHeader('Access-Control-Allow-Credentials', 'true');

    if (req.method === 'OPTIONS') {
        res.setHeader('Access-Control-Allow-Methods', 'HEAD, GET, POST, PUT, PATCH, DELETE');
        res.setHeader('Access-Control-Allow-Headers', 'Origin, Content-Type');

        res.writeHead(200, res.getHeaders());
    }

    console.log('========== RESPONSE =========');
    console.log(req.method);
    console.log(req.url);
    console.log(res.statusCode);
    console.log('=============================');
    console.log();
});

proxy.on('proxyReq', (proxyReq, req) => {
    console.log('========== REQUEST ==========');
    console.log(req.method);
    console.log(req.url);
    console.log('=============================');
    console.log();
});

var server = http.createServer((req, res) => {
    proxy.web(req, res);
});

console.log('listening on port 8000');
server.listen(8000);
