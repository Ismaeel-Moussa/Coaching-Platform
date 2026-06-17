import React, { useState } from 'react';
import { Form, Input, Button, Result } from 'antd';
import { Link } from 'react-router-dom';
import { useForgotPassword } from '../../../hooks/useAuth/useAuth';
import type { ForgotPasswordForm } from '../../../types/auth';
import './ForgotPassword.scss';

const ForgotPassword: React.FC = () => {
  const [form] = Form.useForm<ForgotPasswordForm>();
  const [submitted, setSubmitted] = useState(false);
  const [submittedEmail, setSubmittedEmail] = useState('');
  const { mutate: sendReset, isPending } = useForgotPassword();

  const onFinish = (values: ForgotPasswordForm) => {
    setSubmittedEmail(values.email);
    sendReset(values, {
      onSuccess: () => setSubmitted(true),
    });
  };

  if (submitted) {
    return (
      <div className="forgot-password forgot-password--success" id="forgot-password-page">
        <Result
          icon={
            <div className="forgot-password__success-icon">
              <span className="material-symbols-outlined">mark_email_read</span>
            </div>
          }
          title={<span className="forgot-password__success-title">Check Your Inbox</span>}
          subTitle={
            <span className="forgot-password__success-sub">
              If an account exists for <strong>{submittedEmail}</strong>, you'll receive a reset
              link shortly.
            </span>
          }
          extra={
            <Link to="/sign-in" className="forgot-password__back-btn">
              <span className="material-symbols-outlined">arrow_back</span>
              Back to Sign In
            </Link>
          }
        />
      </div>
    );
  }

  return (
    <div className="forgot-password" id="forgot-password-page">
      <div className="forgot-password__header">
        <div className="forgot-password__icon-wrap">
          <span className="material-symbols-outlined">lock_reset</span>
        </div>
        <h2 className="forgot-password__title">Reset Password</h2>
        <p className="forgot-password__subtitle">
          Enter your email and we'll send you a reset link
        </p>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        requiredMark={false}
        className="forgot-password__form"
      >
        <Form.Item
          name="email"
          label="Email Address"
          rules={[
            { required: true, message: 'Email is required' },
            { type: 'email', message: 'Please enter a valid email address' },
          ]}
        >
          <Input
            id="forgot-password-email"
            size="large"
            placeholder="you@jokernutrition.com"
            prefix={<span className="material-symbols-outlined forgot-password__field-icon">mail</span>}
            autoComplete="email"
            autoFocus
          />
        </Form.Item>

        <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
          <Button
            id="forgot-password-submit"
            type="primary"
            htmlType="submit"
            size="large"
            loading={isPending}
            block
            className="forgot-password__submit-btn"
          >
            {isPending ? 'Sending...' : 'Send Reset Link'}
          </Button>
        </Form.Item>
      </Form>

      <p className="forgot-password__back-hint">
        <Link to="/sign-in" className="forgot-password__back-link">
          <span className="material-symbols-outlined">arrow_back</span>
          Back to Sign In
        </Link>
      </p>
    </div>
  );
};

export default ForgotPassword;
