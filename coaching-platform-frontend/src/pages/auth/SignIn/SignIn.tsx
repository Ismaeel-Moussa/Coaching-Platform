import React from 'react';
import { Form, Input, Button, Divider } from 'antd';
import { Link } from 'react-router-dom';
import { useLogin } from '../../../hooks/useAuth/useAuth';
import type { LoginForm } from '../../../types/auth';
import './SignIn.scss';

const SignIn: React.FC = () => {
  const [form] = Form.useForm<LoginForm>();
  const { mutate: login, isPending } = useLogin();

  const onFinish = (values: LoginForm) => {
    login(values);
  };

  return (
    <div className="sign-in" id="sign-in-page">
      {/* Page header */}
      <div className="sign-in__header">
        <h2 className="sign-in__title">Welcome Back</h2>
        <p className="sign-in__subtitle">Sign in to your Joker Nutrition account</p>
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
          label="Email Address"
          rules={[
            { required: true, message: 'Email is required' },
            { type: 'email', message: 'Please enter a valid email address' },
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
              <span>Password</span>
              <Link to="/forgot-password" className="sign-in__forgot-link" tabIndex={-1}>
                Forgot password?
              </Link>
            </div>
          }
          rules={[
            { required: true, message: 'Password is required' },
            { min: 6, message: 'Password must be at least 6 characters' },
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
            {isPending ? 'Signing in...' : 'Sign In'}
          </Button>
        </Form.Item>
      </Form>

      <Divider className="sign-in__divider" />

      <p className="sign-in__register-hint">
        New to Joker Nutrition?{' '}
        <span className="sign-in__register-note">You need an invitation from your coach.</span>
      </p>
    </div>
  );
};

export default SignIn;
