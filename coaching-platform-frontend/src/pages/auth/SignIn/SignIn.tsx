import React from 'react';
import { Form, Input, Button, Divider } from 'antd';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useLogin } from '../../../hooks/useAuth/useAuth';
import type { LoginForm } from '../../../types/auth';
import './SignIn.scss';

const SignIn: React.FC = () => {
  const { t } = useTranslation();
  const [form] = Form.useForm<LoginForm>();
  const { mutate: login, isPending } = useLogin();

  const onFinish = (values: LoginForm) => {
    login(values);
  };

  return (
    <div className="sign-in" id="sign-in-page">
      {/* Page header */}
      <div className="sign-in__header">
        <h2 className="sign-in__title">{t('auth:signIn.title')}</h2>
        <p className="sign-in__subtitle">{t('auth:signIn.subtitle')}</p>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        requiredMark={false}
        className="sign-in__form"
      >
        {/* Email */}
        <Form.Item
          name="email"
          label={t('auth:signIn.emailLabel')}
          rules={[
            { required: true, message: t('auth:signIn.emailReq') },
            { type: 'email', message: t('auth:signIn.emailInvalid') },
          ]}
        >
          <Input
            id="sign-in-email"
            size="large"
            placeholder="you@jokernutrition.com"
            prefix={<span className="material-symbols-outlined sign-in__field-icon">mail</span>}
            autoComplete="email"
            autoFocus
          />
        </Form.Item>

        {/* Password */}
        <Form.Item
          name="password"
          label={
            <div className="sign-in__password-label">
              <span>{t('auth:signIn.passwordLabel')}</span>
              <Link to="/forgot-password" className="sign-in__forgot-link" tabIndex={-1}>
                {t('auth:signIn.forgotPassword')}
              </Link>
            </div>
          }
          rules={[
            { required: true, message: t('auth:signIn.passwordReq') },
            { min: 6, message: t('auth:signIn.passwordMin') },
          ]}
        >
          <Input.Password
            id="sign-in-password"
            size="large"
            placeholder="••••••••"
            prefix={<span className="material-symbols-outlined sign-in__field-icon">lock</span>}
            autoComplete="current-password"
          />
        </Form.Item>

        {/* Submit */}
        <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
          <Button
            id="sign-in-submit"
            type="primary"
            htmlType="submit"
            size="large"
            loading={isPending}
            block
            className="sign-in__submit-btn"
          >
            {isPending ? t('auth:signIn.submitBtnPending') : t('auth:signIn.submitBtn')}
          </Button>
        </Form.Item>
      </Form>

      <Divider className="sign-in__divider" />

      <p className="sign-in__register-hint">
        {t('auth:signIn.newHint')}{' '}
        <span className="sign-in__register-note">{t('auth:signIn.newHintNote')}</span>
      </p>
    </div>
  );
};

export default SignIn;
