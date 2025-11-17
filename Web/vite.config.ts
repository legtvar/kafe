import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"
import tsconfigPaths from "vite-tsconfig-paths"
import basicSsl from "@vitejs/plugin-basic-ssl"

export default defineConfig({
    plugins: [react({}), tsconfigPaths({}), basicSsl({
      name: "KAFE",
      domains: ["localhost"],
      certDir: "./certs"
    })],
    build: {
        outDir: "build"
    },
})
