import {
    Code,
    Divider,
    Heading,
    Link,
    ListItem,
    OrderedList,
    Text,
    UnorderedList,
} from "@chakra-ui/layout";
import {
    Image,
    Table,
    Tbody,
    Td,
    Th,
    Thead,
    Tr,
    chakra,
} from "@chakra-ui/react";
import Markdown, { Options } from "react-markdown";

export function ChakraMarkdown(props: Partial<Options>) {
    return (
        <Markdown
            components={{
                h1: (props) => <Heading as="h1" size="xl" mb={2} {...props} />,
                h2: (props) => <Heading as="h2" size="lg" mb={2} {...props} />,
                h3: (props) => <Heading as="h3" size="md" mb={2} {...props} />,
                h4: (props) => <Heading as="h4" size="1em" mb={2} {...props} />,
                ol: (props) => <OrderedList listStylePos="inside" {...props} />,
                ul: (props) => (
                    <UnorderedList listStylePos="inside" {...props} />
                ),
                li: (props) => <ListItem {...props} />,
                p: (props) => <Text mb={2}>{props.children}</Text>,
                em: (props) => <Text as="em">{props.children}</Text>,
                blockquote: (props) => (
                    <Code as="blockquote" p={2}>
                        {props.children}
                    </Code>
                ),
                code: (props) => <Code p={2} children={props.children} />,
                del: (props) => <Text as="del">{props.children}</Text>,
                hr: (props) => <Divider />,
                a: Link,
                img: Image,
                text: (props) => <Text as="span">{props.children}</Text>,
                pre: (props) => <chakra.pre>{props.children}</chakra.pre>,
                table: Table,
                thead: Thead,
                tbody: Tbody,
                tr: (props) => <Tr>{props.children}</Tr>,
                td: (props) => <Td>{props.children}</Td>,
                th: (props) => <Th>{props.children}</Th>,
            }}
            skipHtml
            {...props}
        />
    );
}
