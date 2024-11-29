# Debugging production

Changed the image API runs on to the .NET SDK, so that the `dotnet` command would be available in production.

Now `dotnet dev-cert` now runs at publish time.
This is so that API can run at `https` even when behind nginx reverse proxy.
`Https` was necessary due to the way the OpenIdConnect middleware works under the hood -- it obtains the protocol for the redirect URL from the current request, which was was always `http` up until this point.
The ISP expected `https` thus the easiest solution was to ensure API runs on `https` as well.

The `vsdbg` debugger was also added to the Docker image.
Thanks to this, one can attach the vscode debugger directly to the container running in the `lemma-new` VM:

```json
{
  "name": "Attach to Production",
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickRemoteProcess}",
  "pipeTransport": {
    "pipeCwd": "${workspaceFolder}",
    "pipeProgram": "ssh",
    "pipeArgs": [
      "-T",
      "lemma-new",
      "docker",
      "exec",
      "-i",
      "kafe_api_1",
      "sh",
      "-c"
    ],
    "debuggerPath": "~/vsdbg/vsdbg"
  },
  "sourceFileMap": {
    "/kafe/src": "${workspaceRoot}"
  },
  "justMyCode": false
}
```

> The sample above assumes you have `lemma-new` in your ssh config file.
