import React from 'react';
import { Form, Input, Button } from 'antd';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useResetPassword } from '../../../hooks/useAuth/useAuth';
import type { ResetPasswordForm } from '../../../types/auth';
import './ResetPassword.scss';

const ResetPassword: React.FC = () => {
  const { t } = useTranslation();
  const [form] = Form.useForm<ResetPasswordForm>();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { mutate: resetPassword, isPending } = useResetPassword();

  const onFinish = (values: ResetPasswordForm) => {
    resetPassword({ ...values, token });
  };

  return (
    <div className="reset-password" id="reset-password-page">
      <div className="reset-password__header">
        <div className="reset-password__icon-wrap">
          <span className="material-symbols-outlined">key</span>
        </div>
        <h2 className="reset-password__title">{t('auth:resetPassword.title')}</h2>
        <p className="reset-password__subtitle">{t('auth:resetPassword.subtitle')}</p>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        requiredMark={false}
        className="reset-password__form"
      >
        <Form.Item
          name="newPassword"
          label={t('auth:resetPassword.newPasswordLabel')}
          rules={[
            { required: true, message: t('auth:resetPassword.newPasswordReq') },
            { min: 8, message: t('auth:resetPassword.newPasswordMin') },
          ]}
          hasFeedback
        >
          <Input.Password
            id="reset-password-new"
            size="large"
            placeholder={t('auth:resetPassword.newPasswordPlaceholder')}
            prefix={<span className="material-symbols-outlined reset-password__field-icon">lock</span>}
            autoComplete="new-password"
            autoFocus
          />
        </Form.Item>

        <Form.Item
          name="confirmPassword"
          label={t('auth:resetPassword.confirmPasswordLabel')}
          dependencies={['newPassword']}
          rules={[
            { required: true, message: t('auth:resetPassword.confirmPasswordReq') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('newPassword') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('auth:resetPassword.confirmPasswordMismatch')));
              },
            }),
          ]}
          hasFeedback
        >
          <Input.Password
            id="reset-password-confirm"
            size="large"
            placeholder={t('auth:resetPassword.confirmPasswordPlaceholder')}
            prefix={<span className="material-symbols-outlined reset-password__field-icon">lock_check</span>}
            autoComplete="new-password"
          />
        </Form.Item>

        <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
          <Button
            id="reset-password-submit"
            type="primary"
            htmlType="submit"
            size="large"
            loading={isPending}
            block
            className="reset-password__submit-btn"
          >
            {isPending ? t('auth:resetPassword.submitBtnPending') : t('auth:resetPassword.submitBtn')}
          </Button>
        </Form.Item>
      </Form>
    </div>
  );
};

export default ResetPassword;
