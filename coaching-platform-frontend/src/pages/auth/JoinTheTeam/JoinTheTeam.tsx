import React, { useEffect } from 'react';
import { Form, Input, Button, Spin, Alert } from 'antd';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useValidateInviteToken, useRegister } from '../../../hooks/useAuth/useAuth';
import type { RegisterForm } from '../../../types/auth';
import './JoinTheTeam.scss';

const JoinTheTeam: React.FC = () => {
  const [form] = Form.useForm<Omit<RegisterForm, 'invitationToken'>>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  // Validate the invite token on page load
  const {
    data: invitation,
    isLoading: isValidating,
    isError: isTokenInvalid,
  } = useValidateInviteToken(token);

  const { mutate: register, isPending } = useRegister();

  // Redirect to invalid invite page if the token is bad
  useEffect(() => {
    if (!token) {
      navigate('/invalid-invite', { replace: true });
    }
  }, [token, navigate]);

  useEffect(() => {
    if (isTokenInvalid) {
      navigate('/invalid-invite', { replace: true });
    }
  }, [isTokenInvalid, navigate]);

  const onFinish = (values: Omit<RegisterForm, 'invitationToken'>) => {
    if (!token) return;
    register({
      ...values,
      invitationToken: token,
    });
  };

  if (isValidating) {
    return (
      <div className="join-team join-team--loading" id="join-team-page">
        <Spin size="large" />
        <p>Validating your invitation...</p>
      </div>
    );
  }

  if (!invitation) return null;

  return (
    <div className="join-team" id="join-team-page">
      {/* Header */}
      <div className="join-team__header">
        <h2 className="join-team__title">Join the Team</h2>
        <p className="join-team__subtitle">Complete your profile to get started</p>
      </div>

      {/* Invite info banner */}
      <Alert
        className="join-team__invite-banner"
        type="info"
        showIcon
        icon={<span className="material-symbols-outlined">mail</span>}
        message={
          <div className="join-team__invite-info">
            <span className="join-team__invite-label">Invited as</span>
            <span className="join-team__invite-role">{invitation.role}</span>
            <span className="join-team__invite-email">{invitation.email}</span>
          </div>
        }
      />

      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        requiredMark={false}
        className="join-team__form"
      >
        {/* Name row */}
        <div className="join-team__name-row">
          <Form.Item
            name="firstName"
            label="First Name"
            rules={[{ required: true, message: 'First name is required' }]}
          >
            <Input
              id="join-team-first-name"
              size="large"
              placeholder="First name"
              autoFocus
            />
          </Form.Item>

          <Form.Item
            name="lastName"
            label="Last Name"
            rules={[{ required: true, message: 'Last name is required' }]}
          >
            <Input
              id="join-team-last-name"
              size="large"
              placeholder="Last name"
            />
          </Form.Item>
        </div>

        {/* Password */}
        <Form.Item
          name="password"
          label="Password"
          rules={[
            { required: true, message: 'Password is required' },
            { min: 8, message: 'Password must be at least 8 characters' },
          ]}
          hasFeedback
        >
          <Input.Password
            id="join-team-password"
            size="large"
            placeholder="Min. 8 characters"
            prefix={<span className="material-symbols-outlined join-team__field-icon">lock</span>}
            autoComplete="new-password"
          />
        </Form.Item>

        {/* Confirm password */}
        <Form.Item
          name="confirmPassword"
          label="Confirm Password"
          dependencies={['password']}
          rules={[
            { required: true, message: 'Please confirm your password' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('password') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error('Passwords do not match'));
              },
            }),
          ]}
          hasFeedback
        >
          <Input.Password
            id="join-team-confirm-password"
            size="large"
            placeholder="Repeat your password"
            prefix={<span className="material-symbols-outlined join-team__field-icon">lock_check</span>}
            autoComplete="new-password"
          />
        </Form.Item>

        {/* Submit */}
        <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
          <Button
            id="join-team-submit"
            type="primary"
            htmlType="submit"
            size="large"
            loading={isPending}
            block
            className="join-team__submit-btn"
          >
            {isPending ? 'Creating Account...' : 'Create My Account'}
          </Button>
        </Form.Item>
      </Form>

      <p className="join-team__signin-hint">
        Already have an account?{' '}
        <a href="/sign-in">Sign in here</a>
      </p>
    </div>
  );
};

export default JoinTheTeam;
