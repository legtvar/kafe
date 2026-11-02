import {
    Box,
    BoxProps,
    Text,
} from '@chakra-ui/react';
import { useRef, useEffect, useMemo } from "react";

interface IGenerativeScriptProps extends BoxProps {
    code: string
}

export function GenerativeScript({ code }: IGenerativeScriptProps)
{
    const htmlContent = useMemo(() => `
    <!DOCTYPE html>
    <html>
        <head>
        <meta charset="utf-8">
        <style>
            body {
            margin: 0;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            background-color: #121212;
            overflow: hidden;
            }
        </style>
        <!-- Load p5.js from CDN -->
        <script src="https://cdn.jsdelivr.net/npm/p5@2.3.3/lib/p5.min.js "></script>
        </head>
        <body>
        <script>
            try {
            ${code}
            } catch (err) {
            document.body.innerHTML = \`<div style="color: #ff6b6b; font-family: monospace; padding: 20px; background: #2d1515; border-radius: 8px;">
                <strong>p5.js Runtime Error:</strong><br/>\${err.message}
            </div>\`;
            }
        </script>
        </body>
    </html>
    `, [code]);

    return (<Box 
                width={'80%'}
                height={'80%'}
                position="relative"
                background="black"
                outline={0}>
                <iframe 
                    srcDoc={htmlContent}
                    title='p5.js-sketch'
                    sandbox='allow-scripts'
                    width='100%'
                    height='100%'
                />
            </Box>);
}