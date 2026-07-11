import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ConfigProvider } from 'antd';
import enUS from 'antd/locale/en_US';
import arEG from 'antd/locale/ar_EG';

type Language = 'en' | 'ar';
type Direction = 'ltr' | 'rtl';

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  direction: Direction;
  isRTL: boolean;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { i18n } = useTranslation();
  const [language, setLangState] = useState<Language>(() => {
    return (i18n.resolvedLanguage as Language) || 'en';
  });

  const direction: Direction = language === 'ar' ? 'rtl' : 'ltr';
  const isRTL = direction === 'rtl';

  const setLanguage = (lang: Language) => {
    i18n.changeLanguage(lang);
    setLangState(lang);
  };

  useEffect(() => {
    document.documentElement.lang = language;
    document.documentElement.dir = direction;
    // Set class on body for easier scss styling if needed
    if (isRTL) {
      document.body.classList.add('rtl');
      document.body.classList.remove('ltr');
    } else {
      document.body.classList.add('ltr');
      document.body.classList.remove('rtl');
    }
  }, [language, direction, isRTL]);

  // Ant Design locale selection
  const antdLocale = language === 'ar' ? arEG : enUS;

  return (
    <LanguageContext.Provider value={{ language, setLanguage, direction, isRTL }}>
      <ConfigProvider direction={direction} locale={antdLocale}>
        {children}
      </ConfigProvider>
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};
