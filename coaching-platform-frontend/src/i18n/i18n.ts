import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import locales
import enCommon from './locales/en/common.json';
import enAuth from './locales/en/auth.json';
import enAthlete from './locales/en/athlete.json';
import enCoach from './locales/en/coach.json';
import enProfile from './locales/en/profile.json';
import enAdmin from './locales/en/admin.json';

import arCommon from './locales/ar/common.json';
import arAuth from './locales/ar/auth.json';
import arAthlete from './locales/ar/athlete.json';
import arCoach from './locales/ar/coach.json';
import arProfile from './locales/ar/profile.json';
import arAdmin from './locales/ar/admin.json';

const resources = {
  en: {
    common: enCommon,
    auth: enAuth,
    athlete: enAthlete,
    coach: enCoach,
    profile: enProfile,
    admin: enAdmin,
  },
  ar: {
    common: arCommon,
    auth: arAuth,
    athlete: arAthlete,
    coach: arCoach,
    profile: arProfile,
    admin: arAdmin,
  },
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    ns: ['common', 'auth', 'athlete', 'coach', 'profile', 'admin'],
    defaultNS: 'common',
    interpolation: {
      escapeValue: false, // React already safes from XSS
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

// Setup initial html dir and lang attribute
const initialLang = i18n.resolvedLanguage || 'en';
document.documentElement.lang = initialLang;
document.documentElement.dir = initialLang === 'ar' ? 'rtl' : 'ltr';

export default i18n;
