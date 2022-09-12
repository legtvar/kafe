import { Button, NonIdealState, NonIdealStateIconSize, Spinner } from '@blueprintjs/core';
import { useTranslation } from 'react-i18next';

function App() {
    const { t, i18n } = useTranslation();

    return (
        <div
            style={{
                position: 'fixed',
                top: 0,
                left: 0,
                bottom: 0,
                right: 0,
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
            }}
        >
            <NonIdealState
                icon={<Spinner size={NonIdealStateIconSize.STANDARD} />}
                title={t('common:beta.title')}
                description={t('common:beta.description')}
                action={
                    <Button
                        outlined={true}
                        text={t('common:beta.toggleLanguage', {
                            language: i18n.language === 'cs' ? 'en' : 'cs',
                        })}
                        icon="translate"
                        intent="primary"
                        onClick={() => i18n.changeLanguage(i18n.language === 'cs' ? 'en' : 'cs')}
                    />
                }
            />
        </div>
    );
}

export default App;
