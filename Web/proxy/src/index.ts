import fs from 'fs';
import httpProxy from 'http-proxy';
import https from 'https';
import selfsigned from 'selfsigned';

const sourcePort = 44369;
const sourceAddress = 'localhost';
const targetPort = 443;
const targetAddress = 'kafe-stage.fi.muni.cz';

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
    secure: false,
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

if (!fs.existsSync('certs/cert.pem') || !fs.existsSync('certs/key.pem')) {
    const ss = selfsigned.generate(undefined, { clientCertificateKeySize: 4096, keySize: 4096 });
    fs.writeFileSync('certs/cert.pem', ss.cert);
    fs.writeFileSync('certs/key.pem', ss.private);
}

var server = https.createServer(
    {
        cert: fs.readFileSync('certs/cert.pem'),
        key: fs.readFileSync('certs/key.pem'),
    },
    (req, res) => {
        proxy.web(req, res);
    },
);

console.log('===== Proxy running =====');
console.log(`${sourceAddress}:${sourcePort} <-> ${targetAddress}:${targetPort}`);
console.log('Press ENTER to pause/resume proxy');
console.log('');

process.openStdin().addListener('data', (data) => {
    if (data.toString().trim() === '') {
        if (server.listening) {
            server.close();
            console.log('===== Proxy paused =====');
            console.log('Press ENTER to pause/resume proxy');
            console.log('');
        } else {
            server.listen(sourcePort);
            console.log('===== Proxy resumed =====');
            console.log('Press ENTER to pause/resume proxy');
            console.log('');
        }
    }
});

server.listen(sourcePort);
