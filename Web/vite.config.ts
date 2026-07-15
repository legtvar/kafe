import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"
import tsconfigPaths from "vite-tsconfig-paths"
import basicSsl from "@vitejs/plugin-basic-ssl"
import fs from "fs"

export default defineConfig(() => {
    const basicSslPlugin = basicSsl({
        name: "KAFE",
        domains: [
            "localhost",
            "*.dev.localhost",
            "*.dev.internal",
            "host.docker.internal",
            "host.containers.internal",
            "127.0.0.1",
            "0000:0000:0000:0000:0000:0000:0000:0001"
        ],
        certDir: "./certs"
    });

    const hasDotnetCerts = fs.existsSync("./certs/_dotnet.pem") && fs.existsSync("./certs/_dotnet.key");
    if (hasDotnetCerts) {
        console.log("Using .NET's dev https certificates.");
    } else {
        console.log("Using or generating custom self-signed https certificates.");
    }

    return {
        plugins: [react({}), tsconfigPaths({}), ...(hasDotnetCerts ? [] : [basicSslPlugin])],
        server: {
            https: !hasDotnetCerts ? undefined : { cert: "./certs/_dotnet.pem", key: "./certs/_dotnet.key" }
        },
        build: {
            outDir: "build"
        },
    };
})
