import React from 'react';
import { Form, Input, Button } from 'antd';
import { useSearchParams } from 'react-router-dom';
import { useResetPassword } from '../../../hooks/useAuth/useAuth';
import type { ResetPasswordForm } from '../../../types/auth';
import './ResetPassword.scss';

const ResetPassword: React.FC = () => {
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
        <h2 className="reset-password__title">Set New Password</h2>
        <p className="reset-password__subtitle">Choose a strong password for your account</p>
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
          label="New Password"
          rules={[
            { required: true, message: 'Password is required' },
            { min: 8, message: 'Password must be at least 8 characters' },
          ]}
          hasFeedback
        >
          <Input.Password
            id="reset-password-new"
            size="large"
            placeholder="Min. 8 characters"
            prefix={<span className="material-symbols-outlined reset-password__field-icon">lock</span>}
            autoComplete="new-password"
            autoFocus
          />
        </Form.Item>

        <Form.Item
          name="confirmPassword"
          label="Confirm New Password"
          dependencies={['newPassword']}
          rules={[
            { required: true, message: 'Please confirm your password' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('newPassword') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error('Passwords do not match'));
              },
            }),
          ]}
          hasFeedback
        >
          <Input.Password
            id="reset-password-confirm"
            size="large"
            placeholder="Repeat new password"
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
            {isPending ? 'Updating...' : 'Update Password'}
          </Button>
        </Form.Item>
      </Form>
    </div>
  );
};

export default ResetPassword;
