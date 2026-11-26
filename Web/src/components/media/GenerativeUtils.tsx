import { useApi } from '@/hooks/Caffeine';
import {
    Box,
    BoxProps,
    Text,
} from '@chakra-ui/react';
import { useMemo } from "react";
import JSZip from 'jszip';

export async function loadScripts(zipBlob: Blob): Promise<string> {
    const archive = await JSZip.loadAsync(zipBlob);
    
    const files = await Promise.all(Object.entries(archive.files).filter(([path, entry]) =>
        !entry.dir && path.endsWith(".js") && !path.endsWith("p5.js") && !path.endsWith("p5.min.js") && !path.endsWith("p5.sound.min.js")
    ).map(([, entry]) => entry.async('string')));

    return files.map((script) => `<script>${script}</script>`).join('');
}

interface IGenerativeScriptProps extends BoxProps {
    scripts: string
}

export function GenerativeScript({ scripts }: IGenerativeScriptProps)
{
    const htmlContent = useMemo(() => {
        return `
            <!DOCTYPE html>
            <html lang="en">
                <head>
                    <script src="https://cdn.jsdelivr.net/npm/p5@2.3.2/lib/p5.js"></script>
                    <script src="https://cdn.jsdelivr.net/npm/p5.sound@0.4.1/dist/p5.sound.min.js"></script>
                    <meta charset="utf-8" />
                    <style>
                        body {
                            margin: 0;
                        }
                        .p5Canvas {
                            display: block;
                            border: none;
                        }
                    </style>
                </head>
                <body>
                    ${scripts}
                </body>
            </html>
            `}, [scripts]);

    return (<Box 
                width={'80%'}
                height={'80%'}
                background="black">
                    <iframe 
                        srcDoc={htmlContent}
                        title='p5.js-sketch'
                        sandbox='allow-scripts'
                        width='100%'
                        height='100%'
                    />
            </Box>);
}