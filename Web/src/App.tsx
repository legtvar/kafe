import { TranslationOutlined } from '@ant-design/icons';
import { Button, Result } from 'antd';
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
            <Result
                status="404"
                title={t('common:beta.title')}
                subTitle={t('common:beta.description')}
                extra={
                    <Button
                        type="default"
                        onClick={() => i18n.changeLanguage(i18n.language === 'cs' ? 'en' : 'cs')}
                        icon={<TranslationOutlined />}
                    >
                        {t('common:beta.toggleLanguage', {
                            language: i18n.language === 'cs' ? 'en' : 'cs',
                        })}
                    </Button>
                }
            />
        </div>
    );
}

export default App;
