import { Heading, ListItem, OrderedList, UnorderedList } from '@chakra-ui/layout';
import ChakraUIRenderer from 'chakra-ui-markdown-renderer';
import Markdown, { Options } from 'react-markdown';

export function ChakraMarkdown(props: Partial<Options>) {
    return (
        <Markdown
            components={ChakraUIRenderer({
                h1: (props) => <Heading as="h1" size="xl" mb={2} {...props} />,
                h2: (props) => <Heading as="h2" size="lg" mb={2} {...props} />,
                h3: (props) => <Heading as="h3" size="md" mb={2} {...props} />,
                h4: (props) => <Heading as="h4" size="1em" mb={2} {...props} />,
                ol: (props) => <OrderedList listStylePos="inside" {...props} />,
                ul: (props) => <UnorderedList listStylePos="inside" {...props} />,
                li: (props) => <ListItem {...props} />,
            })}
            skipHtml
            {...props}
        />
    );
}
