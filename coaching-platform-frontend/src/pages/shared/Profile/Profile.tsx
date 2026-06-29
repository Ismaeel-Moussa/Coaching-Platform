import React, { useState, useEffect } from 'react';
import { Spin, Alert } from 'antd';
import { useGetProfile, useUpdateProfile, useChangePassword, useUploadAvatar } from '../../../hooks/useProfile/useProfile';
import './Profile.scss';

const Profile: React.FC = () => {
  const { data: profile, isLoading, error } = useGetProfile();
  const updateProfileMutation = useUpdateProfile();
  const changePasswordMutation = useChangePassword();
  const uploadAvatarMutation = useUploadAvatar();

  const [activeTab, setActiveTab] = useState<'info' | 'password' | 'stats'>('info');

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      uploadAvatarMutation.mutate(file, {
        onSuccess: (data) => {
          setProfilePictureUrl(data.url);
        }
      });
    }
  };

  // Form states for profile update
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [profilePictureUrl, setProfilePictureUrl] = useState('');
  const [bio, setBio] = useState('');
  const [weightKg, setWeightKg] = useState<number | ''>('');
  const [heightCm, setHeightCm] = useState<number | ''>('');
  const [targetGoal, setTargetGoal] = useState('');

  // Form states for password change
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordError, setPasswordError] = useState('');

  // Sync profile data to local state on load
  useEffect(() => {
    if (profile) {
      setFirstName(profile.firstName || '');
      setLastName(profile.lastName || '');
      setProfilePictureUrl(profile.profilePictureUrl || '');
      setBio(profile.bio || '');
      setWeightKg(profile.weightKg ?? '');
      setHeightCm(profile.heightCm ?? '');
      setTargetGoal(profile.targetGoal || '');
    }
  }, [profile]);

  const handleUpdateProfile = (e: React.FormEvent) => {
    e.preventDefault();
    if (!firstName.trim() || !lastName.trim()) return;

    updateProfileMutation.mutate({
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      profilePictureUrl: profilePictureUrl.trim() || null,
      bio: profile?.role !== 'Athlete' ? bio.trim() || null : undefined,
      weightKg: profile?.role === 'Athlete' ? (weightKg === '' ? null : Number(weightKg)) : undefined,
      heightCm: profile?.role === 'Athlete' ? (heightCm === '' ? null : Number(heightCm)) : undefined,
      targetGoal: profile?.role === 'Athlete' ? targetGoal.trim() || null : undefined,
    });
  };

  const handleChangePassword = (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError('');

    if (!currentPassword || !newPassword || !confirmPassword) {
      setPasswordError('All password fields are required.');
      return;
    }

    if (newPassword.length < 8) {
      setPasswordError('New password must be at least 8 characters long.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setPasswordError('New password and confirmation do not match.');
      return;
    }

    changePasswordMutation.mutate(
      {
        currentPassword,
        newPassword,
        confirmPassword,
      },
      {
        onSuccess: () => {
          setCurrentPassword('');
          setNewPassword('');
          setConfirmPassword('');
        },
      }
    );
  };

  if (isLoading) {
    return (
      <div className="profile-page-loader">
        <Spin size="large" />
        <div className="profile-page-loader__text">Loading Profile Details...</div>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="profile-page-error">
        <Alert
          message="Error loading profile"
          description="Could not load your profile details. Please try reloading the page."
          type="error"
          showIcon
        />
      </div>
    );
  }

  const userInitials = `${profile.firstName?.[0] || ''}${profile.lastName?.[0] || ''}`;

  return (
    <div className="profile-page">
      {/* ── Header Card ── */}
      <header className="profile-page__header">
        <div className="profile-page__avatar-wrapper">
          <div className="profile-page__avatar">
            {profilePictureUrl ? (
              <img src={profilePictureUrl} alt="User Avatar" className="profile-page__avatar-img" />
            ) : (
              <span className="profile-page__avatar-initials">{userInitials}</span>
            )}
            
            {/* Loading spinner overlay */}
            {uploadAvatarMutation.isPending && (
              <div className="profile-page__avatar-loading">
                <Spin size="small" />
              </div>
            )}
          </div>
          
          {/* File input overlay trigger */}
          <label htmlFor="avatar-file-input" className="profile-page__avatar-trigger" title="Upload Photo">
            <span className="material-symbols-outlined">photo_camera</span>
            <input
              id="avatar-file-input"
              type="file"
              accept="image/*"
              style={{ display: 'none' }}
              onChange={handleFileChange}
              disabled={uploadAvatarMutation.isPending}
            />
          </label>
        </div>
        <div className="profile-page__header-info">
          <h1 className="profile-page__name">{profile.firstName} {profile.lastName}</h1>
          <p className="profile-page__role-tag">{profile.role}</p>
          <p className="profile-page__email">
            <span className="material-symbols-outlined">mail</span>
            {profile.email}
          </p>
        </div>
      </header>

      {/* ── Tabs Selector ── */}
      <nav className="profile-page__tabs">
        <button
          className={`profile-page__tab-btn ${activeTab === 'info' ? 'profile-page__tab-btn--active' : ''}`}
          onClick={() => setActiveTab('info')}
        >
          <span className="material-symbols-outlined">manage_accounts</span>
          Edit Info
        </button>
        <button
          className={`profile-page__tab-btn ${activeTab === 'stats' ? 'profile-page__tab-btn--active' : ''}`}
          onClick={() => setActiveTab('stats')}
        >
          <span className="material-symbols-outlined">query_stats</span>
          Insights
        </button>
        <button
          className={`profile-page__tab-btn ${activeTab === 'password' ? 'profile-page__tab-btn--active' : ''}`}
          onClick={() => setActiveTab('password')}
        >
          <span className="material-symbols-outlined">lock_reset</span>
          Change Password
        </button>
      </nav>

      {/* ── Content Area ── */}
      <div className="profile-page__content">
        {/* Tab 1: Edit Profile */}
        {activeTab === 'info' && (
          <form className="profile-page__form fade-in" onSubmit={handleUpdateProfile}>
            <h2 className="profile-page__section-title">Personal Details</h2>
            
            <div className="profile-page__form-row">
              <div className="profile-page__form-group">
                <label htmlFor="firstName">First Name</label>
                <input
                  id="firstName"
                  type="text"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  required
                />
              </div>
              <div className="profile-page__form-group">
                <label htmlFor="lastName">Last Name</label>
                <input
                  id="lastName"
                  type="text"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  required
                />
              </div>
            </div>

            {profile.role !== 'Athlete' ? (
              <div className="profile-page__form-group">
                <label htmlFor="bio">Coach Bio</label>
                <textarea
                  id="bio"
                  rows={4}
                  placeholder="Share a short bio with your clients..."
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                />
              </div>
            ) : (
              <>
                <h2 className="profile-page__section-title profile-page__section-title--spacing">
                  Fitness & Body Metrics
                </h2>
                <div className="profile-page__form-row">
                  <div className="profile-page__form-group">
                    <label htmlFor="weightKg">Weight (kg)</label>
                    <input
                      id="weightKg"
                      type="number"
                      step="0.1"
                      placeholder="e.g. 75.5"
                      value={weightKg}
                      onChange={(e) => setWeightKg(e.target.value === '' ? '' : Number(e.target.value))}
                    />
                  </div>
                  <div className="profile-page__form-group">
                    <label htmlFor="heightCm">Height (cm)</label>
                    <input
                      id="heightCm"
                      type="number"
                      step="0.1"
                      placeholder="e.g. 180"
                      value={heightCm}
                      onChange={(e) => setHeightCm(e.target.value === '' ? '' : Number(e.target.value))}
                    />
                  </div>
                </div>
                <div className="profile-page__form-group">
                  <label htmlFor="targetGoal">Fitness Target / Goal</label>
                  <input
                    id="targetGoal"
                    type="text"
                    placeholder="e.g. Fat Loss / Muscle Gain / Marathon Prep"
                    value={targetGoal}
                    onChange={(e) => setTargetGoal(e.target.value)}
                  />
                </div>
              </>
            )}

            <button
              type="submit"
              className="profile-page__submit-btn"
              disabled={updateProfileMutation.isPending}
            >
              {updateProfileMutation.isPending ? 'Saving Changes...' : 'Save Profile Details'}
            </button>
          </form>
        )}

        {/* Tab 2: Change Password */}
        {activeTab === 'password' && (
          <form className="profile-page__form fade-in" onSubmit={handleChangePassword}>
            <h2 className="profile-page__section-title">Change Password</h2>

            {passwordError && (
              <div className="profile-page__alert profile-page__alert--error">
                <span className="material-symbols-outlined">error</span>
                <span>{passwordError}</span>
              </div>
            )}

            <div className="profile-page__form-group">
              <label htmlFor="currentPassword">Current Password</label>
              <input
                id="currentPassword"
                type="password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
              />
            </div>

            <div className="profile-page__form-group">
              <label htmlFor="newPassword">New Password</label>
              <input
                id="newPassword"
                type="password"
                placeholder="At least 8 characters"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
            </div>

            <div className="profile-page__form-group">
              <label htmlFor="confirmPassword">Confirm New Password</label>
              <input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              className="profile-page__submit-btn"
              disabled={changePasswordMutation.isPending}
            >
              {changePasswordMutation.isPending ? 'Updating Password...' : 'Update Password'}
            </button>
          </form>
        )}

        {/* Tab 3: Insights & Statistics */}
        {activeTab === 'stats' && (
          <div className="profile-page__stats fade-in">
            <h2 className="profile-page__section-title">Account Insights</h2>
            
            {profile.role === 'Athlete' ? (
              <div className="profile-page__stats-grid">
                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">local_fire_department</span>
                  <div className="profile-page__stat-value">{profile.currentStreak ?? 0} days</div>
                  <div className="profile-page__stat-label">Current Check-in Streak</div>
                </div>

                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">emoji_events</span>
                  <div className="profile-page__stat-value">{profile.longestStreak ?? 0} days</div>
                  <div className="profile-page__stat-label">Longest All-Time Streak</div>
                </div>

                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">sports</span>
                  <div className="profile-page__stat-value">
                    {profile.assignedCoachName || 'No Coach Assigned'}
                  </div>
                  <div className="profile-page__stat-label">Assigned Coach</div>
                </div>

                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">monitoring</span>
                  <div className="profile-page__stat-value">
                    {profile.weightKg ? `${profile.weightKg} kg` : 'Not Set'}
                  </div>
                  <div className="profile-page__stat-label">Current Weight</div>
                </div>
              </div>
            ) : (
              <div className="profile-page__stats-grid">
                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">verified_user</span>
                  <div className="profile-page__stat-value">Active Portal</div>
                  <div className="profile-page__stat-label">Role: {profile.role}</div>
                </div>

                <div className="profile-page__stat-card">
                  <span className="material-symbols-outlined profile-page__stat-icon">school</span>
                  <div className="profile-page__stat-value">JN Staff</div>
                  <div className="profile-page__stat-label">Coaching & Nutrition Hub</div>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Profile;
