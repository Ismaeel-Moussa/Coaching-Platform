import React, { useState, useEffect, useCallback } from 'react';
import {
  Table,
  Button,
  Modal,
  Form,
  Input,
  Select,
  Tag,
  Badge,
  Drawer,
  Card,
  Row,
  Col,
  Tooltip,
  Space,
  Avatar,
  Statistic,
  Spin,
  message,
} from 'antd';
import { useTranslation } from 'react-i18next';
import { useLanguage } from '../../../contexts/LanguageContext';
import {
  getUsers,
  getMonitoringSummary,
  getCoachDeactivationImpact,
  toggleUserStatus,
  getUserLoginAuditLogs,
  exportUserAuditLogsCsv,
} from '../../../api/adminApi';
import type {
  UserManagementDto,
  UserMonitoringSummaryDto,
  CoachDeactivationImpactDto,
  UserLoginAuditLogDto,
  UserFilterParams,
} from '../../../types/admin';
import './UserManagement.scss';

const { Option } = Select;

const DEACTIVATION_REASONS = [
  { value: 'Subscription Expired', labelKey: 'admin:users.reasons.subscriptionExpired', fallback: 'Subscription Expired' },
  { value: 'Terms of Service Violation', labelKey: 'admin:users.reasons.termsViolation', fallback: 'Terms of Service Violation' },
  { value: 'Requested Account Suspension', labelKey: 'admin:users.reasons.requestedSuspension', fallback: 'Requested Account Suspension' },
  { value: 'Prolonged Inactivity', labelKey: 'admin:users.reasons.prolongedInactivity', fallback: 'Prolonged Inactivity' },
  { value: 'Custom Reason', labelKey: 'admin:users.reasons.customReason', fallback: 'Custom Reason' },
];

const UserManagement: React.FC = () => {
  const { t } = useTranslation(['admin', 'common']);
  const { isRTL } = useLanguage();

  // Filters & Pagination
  const [search, setSearch] = useState<string>('');
  const [role, setRole] = useState<string>('All');
  const [statusFilter, setStatusFilter] = useState<string>('All');
  const [inactivityFilter, setInactivityFilter] = useState<string>('all');
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);

  // State
  const [users, setUsers] = useState<UserManagementDto[]>([]);
  const [totalUsers, setTotalUsers] = useState<number>(0);
  const [loadingUsers, setLoadingUsers] = useState<boolean>(true);
  const [summary, setSummary] = useState<UserMonitoringSummaryDto | null>(null);
  const [loadingSummary, setLoadingSummary] = useState<boolean>(true);
  const [exportingCsv, setExportingCsv] = useState<boolean>(false);

  // Deactivation Modal State
  const [deactivateModalVisible, setDeactivateModalVisible] = useState<boolean>(false);
  const [selectedUser, setSelectedUser] = useState<UserManagementDto | null>(null);
  const [coachImpact, setCoachImpact] = useState<CoachDeactivationImpactDto | null>(null);
  const [loadingImpact, setLoadingImpact] = useState<boolean>(false);
  const [availableCoaches, setAvailableCoaches] = useState<{ id: number; name: string }[]>([]);
  const [submittingStatus, setSubmittingStatus] = useState<boolean>(false);
  const [form] = Form.useForm();

  // Audit Logs Drawer State
  const [auditDrawerVisible, setAuditDrawerVisible] = useState<boolean>(false);
  const [auditUser, setAuditUser] = useState<UserManagementDto | null>(null);
  const [auditLogs, setAuditLogs] = useState<UserLoginAuditLogDto[]>([]);
  const [loadingAudit, setLoadingAudit] = useState<boolean>(false);

  // Load summary metrics
  const loadSummary = async () => {
    try {
      setLoadingSummary(true);
      const data = await getMonitoringSummary();
      setSummary(data);
    } catch (err) {
      console.error('Failed to load summary stats:', err);
    } finally {
      setLoadingSummary(false);
    }
  };

  // Load users list
  const loadUsers = useCallback(async () => {
    try {
      setLoadingUsers(true);
      const params: UserFilterParams = {
        search,
        role: role === 'All' ? undefined : role,
        isActive: statusFilter === 'All' ? undefined : statusFilter === 'Active',
        inactivityFilter,
        pageNumber: page,
        pageSize,
      };
      const res = await getUsers(params);
      setUsers(res.items);
      setTotalUsers(res.totalCount);
    } catch (err) {
      message.error(t('admin:users.messages.loadFailed', 'Failed to load users list.'));
    } finally {
      setLoadingUsers(false);
    }
  }, [search, role, statusFilter, inactivityFilter, page, pageSize, t]);

  useEffect(() => {
    loadSummary();
  }, []);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  // Fetch available coaches for reassignment dropdown
  const loadAvailableCoaches = async () => {
    try {
      const res = await getUsers({ role: 'Coach', isActive: true, pageSize: 100 });
      setAvailableCoaches(
        res.items.map((u) => ({ id: u.id, name: `${u.firstName} ${u.lastName}`.trim() })),
      );
    } catch (err) {
      console.error('Failed to load coaches:', err);
    }
  };

  // Open Deactivate / Reactivate Modal
  const handleOpenStatusModal = async (userRecord: UserManagementDto) => {
    setSelectedUser(userRecord);
    form.resetFields();

    if (userRecord.isActive) {
      // Deactivating
      if (userRecord.role === 'Coach') {
        setLoadingImpact(true);
        setDeactivateModalVisible(true);
        loadAvailableCoaches();
        try {
          const impact = await getCoachDeactivationImpact(userRecord.id);
          setCoachImpact(impact);
        } catch (err) {
          console.error(err);
        } finally {
          setLoadingImpact(false);
        }
      } else {
        setCoachImpact(null);
        setDeactivateModalVisible(true);
      }
    } else {
      // Reactivating immediately
      Modal.confirm({
        title: t('admin:users.reactivateTitle', 'Reactivate Account'),
        content: t(
          'admin:users.reactivateConfirm',
          'Are you sure you want to reactivate {{name}}\'s account?',
          { name: userRecord.fullName },
        ),
        okText: t('common:actions.confirm', 'Reactivate'),
        onOk: async () => {
          try {
            await toggleUserStatus(userRecord.id, { isActive: true });
            message.success(t('admin:users.messages.reactivated', 'Account reactivated successfully.'));
            loadUsers();
            loadSummary();
          } catch (err: any) {
            message.error(err?.response?.data?.message || 'Failed to reactivate account.');
          }
        },
      });
    }
  };

  // Confirm Deactivation
  const handleConfirmDeactivation = async () => {
    if (!selectedUser) return;
    try {
      const values = await form.validateFields();
      setSubmittingStatus(true);

      const reason =
        values.reasonPreset === 'Custom Reason'
          ? values.customReason
          : values.reasonPreset;

      await toggleUserStatus(selectedUser.id, {
        isActive: false,
        reason,
        reassignCoachId: values.reassignCoachId,
      });

      message.success(t('admin:users.messages.deactivated', 'User account deactivated and active sessions revoked.'));
      setDeactivateModalVisible(false);
      loadUsers();
      loadSummary();
    } catch (err: any) {
      if (err?.response?.data?.message) {
        message.error(err.response.data.message);
      }
    } finally {
      setSubmittingStatus(false);
    }
  };

  // Open Audit Drawer
  const handleOpenAuditDrawer = async (userRecord: UserManagementDto) => {
    setAuditUser(userRecord);
    setAuditDrawerVisible(true);
    setLoadingAudit(true);
    try {
      const logs = await getUserLoginAuditLogs(userRecord.id);
      setAuditLogs(logs);
    } catch (err) {
      message.error(t('admin:users.messages.auditFailed', 'Failed to fetch audit logs.'));
    } finally {
      setLoadingAudit(false);
    }
  };

  // CSV Export
  const handleExportCsv = async () => {
    try {
      setExportingCsv(true);
      const params: UserFilterParams = {
        search,
        role: role === 'All' ? undefined : role,
        isActive: statusFilter === 'All' ? undefined : statusFilter === 'Active',
        inactivityFilter,
      };
      const blob = await exportUserAuditLogsCsv(params);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `user_audit_report_${new Date().toISOString().slice(0, 10)}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      message.success(t('admin:users.messages.csvDownloaded', 'CSV report downloaded.'));
    } catch (err) {
      message.error(t('admin:users.messages.csvFailed', 'Failed to export CSV.'));
    } finally {
      setExportingCsv(false);
    }
  };

  // Format relative timestamp
  const formatLastLogin = (dateStr?: string | null) => {
    if (!dateStr) return <span className="text-muted">{t('admin:users.never', 'Never')}</span>;
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 5) return <span className="login-online">{t('admin:users.justNow', 'Just now')}</span>;
    if (diffMins < 60) return <span>{diffMins} {t('admin:users.minsAgo', 'mins ago')}</span>;
    if (diffHours < 24) return <span>{diffHours} {t('admin:users.hoursAgo', 'hours ago')}</span>;
    if (diffDays === 1) return <span>{t('admin:users.yesterday', 'Yesterday')}</span>;
    return <span>{date.toLocaleDateString()} {date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>;
  };

  const getRoleTag = (userRole: string) => {
    switch (userRole) {
      case 'Admin':
        return <Tag color="purple">{t('admin:users.roles.admin', 'Admin')}</Tag>;
      case 'Coach':
        return <Tag color="blue">{t('admin:users.roles.coach', 'Coach')}</Tag>;
      default:
        return <Tag color="green">{t('admin:users.roles.athlete', 'Athlete')}</Tag>;
    }
  };

  const columns = [
    {
      title: t('admin:users.table.user', 'User'),
      dataIndex: 'fullName',
      key: 'fullName',
      render: (_: any, record: UserManagementDto) => (
        <Space size="middle">
          <Avatar
            src={record.profilePictureUrl}
            style={{ backgroundColor: record.isActive ? '#1890ff' : '#bfbfbf' }}
          >
            {record.firstName?.[0]}
          </Avatar>
          <div>
            <div className="user-name">{record.fullName}</div>
            <div className="user-email">{record.email}</div>
          </div>
        </Space>
      ),
    },
    {
      title: t('admin:users.table.role', 'Role'),
      dataIndex: 'role',
      key: 'role',
      render: (roleVal: string) => getRoleTag(roleVal),
    },
    {
      title: t('admin:users.table.status', 'Status'),
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean, record: UserManagementDto) => (
        record.isActive ? (
          <Badge status="success" text={t('admin:users.active', 'Active')} />
        ) : (
          <Tooltip title={record.deactivationReason || t('admin:users.noReason', 'Deactivated by Admin')}>
            <Badge status="error" text={t('admin:users.deactivated', 'Deactivated')} />
          </Tooltip>
        )
      ),
    },
    {
      title: t('admin:users.table.assignments', 'Assignments'),
      key: 'assignments',
      render: (_: any, record: UserManagementDto) => {
        if (record.role === 'Coach') {
          return <span>{record.assignedAthleteCount} {t('admin:users.athletes', 'athletes')}</span>;
        }
        if (record.role === 'Athlete') {
          return record.assignedCoachName ? (
            <span className="text-muted">{t('admin:users.coach', 'Coach')}: {record.assignedCoachName}</span>
          ) : (
            <span className="text-muted">{t('admin:users.unassigned', 'Unassigned')}</span>
          );
        }
        return <span className="text-muted">—</span>;
      },
    },
    {
      title: t('admin:users.table.lastLogin', 'Last Sign-In'),
      key: 'lastLoginAt',
      render: (_: any, record: UserManagementDto) => (
        <div>
          <div>{formatLastLogin(record.lastLoginAt)}</div>
          {record.lastLoginIp && <div className="user-ip">IP: {record.lastLoginIp}</div>}
        </div>
      ),
    },
    {
      title: t('admin:users.table.actions', 'Actions'),
      key: 'actions',
      render: (_: any, record: UserManagementDto) => (
        <Space size="small">
          <Button
            type={record.isActive ? 'default' : 'primary'}
            danger={record.isActive}
            size="small"
            onClick={() => handleOpenStatusModal(record)}
          >
            {record.isActive ? t('admin:users.deactivate', 'Deactivate') : t('admin:users.reactivate', 'Reactivate')}
          </Button>
          <Button
            size="small"
            onClick={() => handleOpenAuditDrawer(record)}
          >
            {t('admin:users.auditLogs', 'History')}
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="admin-user-management-page">
      <div className="page-header">
        <div>
          <h2>{t('admin:users.title', 'User Management & Sign-In Monitoring')}</h2>
          <p className="page-subtitle">
            {t('admin:users.subtitle', 'Monitor account activities, manage user statuses, and review sign-in audit histories.')}
          </p>
        </div>
        <Button
          type="primary"
          loading={exportingCsv}
          onClick={handleExportCsv}
        >
          {t('admin:users.exportCsv', 'Export Audit CSV')}
        </Button>
      </div>

      {/* Summary Cards */}
      <Row gutter={[16, 16]} className="summary-cards-row">
        <Col xs={24} sm={12} md={4}>
          <Card loading={loadingSummary} className="metric-card">
            <Statistic title={t('admin:users.metrics.total', 'Total Users')} value={summary?.totalUsersCount || 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card loading={loadingSummary} className="metric-card">
            <Statistic title={t('admin:users.metrics.activeCoaches', 'Active Coaches')} value={summary?.activeCoachesCount || 0} valueStyle={{ color: '#1890ff' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card loading={loadingSummary} className="metric-card">
            <Statistic title={t('admin:users.metrics.activeAthletes', 'Active Athletes')} value={summary?.activeAthletesCount || 0} valueStyle={{ color: '#52c41a' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card loading={loadingSummary} className="metric-card">
            <Statistic title={t('admin:users.metrics.deactivated', 'Deactivated')} value={summary?.deactivatedUsersCount || 0} valueStyle={{ color: '#ff4d4f' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card loading={loadingSummary} className="metric-card">
            <Statistic title={t('admin:users.metrics.activeToday', 'Active (24h)')} value={summary?.activeLast24hCount || 0} valueStyle={{ color: '#722ed1' }} />
          </Card>
        </Col>
      </Row>

      {/* Filter Bar */}
      <Card className="filter-card">
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} sm={12} md={7}>
            <Input.Search
              placeholder={t('admin:users.searchPlaceholder', 'Search Name or Email...')}
              allowClear
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onSearch={() => setPage(1)}
            />
          </Col>
          <Col xs={12} sm={6} md={4}>
            <Select value={role} onChange={(val) => { setRole(val); setPage(1); }} style={{ width: '100%' }}>
              <Option value="All">{t('admin:users.filterRoleAll', 'All Roles')}</Option>
              <Option value="Coach">{t('admin:users.filterCoach', 'Coach')}</Option>
              <Option value="Athlete">{t('admin:users.filterAthlete', 'Athlete')}</Option>
              <Option value="Admin">{t('admin:users.filterAdmin', 'Admin')}</Option>
            </Select>
          </Col>
          <Col xs={12} sm={6} md={4}>
            <Select value={statusFilter} onChange={(val) => { setStatusFilter(val); setPage(1); }} style={{ width: '100%' }}>
              <Option value="All">{t('admin:users.filterStatusAll', 'All Statuses')}</Option>
              <Option value="Active">{t('admin:users.filterActive', 'Active Only')}</Option>
              <Option value="Deactivated">{t('admin:users.filterDeactivated', 'Deactivated')}</Option>
            </Select>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Select value={inactivityFilter} onChange={(val) => { setInactivityFilter(val); setPage(1); }} style={{ width: '100%' }}>
              <Option value="all">{t('admin:users.filterInactivityAll', 'All Sign-In Times')}</Option>
              <Option value="24h">{t('admin:users.filter24h', 'Active in Last 24 Hours')}</Option>
              <Option value="30days">{t('admin:users.filter30days', 'Inactive > 30 Days')}</Option>
              <Option value="never">{t('admin:users.filterNever', 'Never Signed In')}</Option>
            </Select>
          </Col>
        </Row>
      </Card>

      {/* Users Data Table */}
      <Card className="table-card">
        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loadingUsers}
          pagination={{
            current: page,
            pageSize,
            total: totalUsers,
            onChange: (p, ps) => {
              setPage(p);
              setPageSize(ps);
            },
          }}
        />
      </Card>

      {/* Deactivation Modal */}
      <Modal
        title={t('admin:users.deactivateModalTitle', 'Deactivate Account & Revoke Sessions')}
        open={deactivateModalVisible}
        onOk={handleConfirmDeactivation}
        confirmLoading={submittingStatus}
        onCancel={() => setDeactivateModalVisible(false)}
        okText={t('admin:users.confirmDeactivate', 'Deactivate Account')}
        okButtonProps={{ danger: true }}
      >
        {selectedUser && (
          <div>
            <p>
              {t(
                'admin:users.deactivateConfirmText',
                'Deactivating {{name}}\'s account will immediately revoke all active refresh tokens and log out any active sessions.',
                { name: selectedUser.fullName },
              )}
            </p>

            {/* Coach Pre-Flight Impact */}
            {selectedUser.role === 'Coach' && (
              <div className="impact-box">
                <h4>{t('admin:users.coachImpactTitle', 'Coach Deactivation Impact')}</h4>
                {loadingImpact ? (
                  <Spin size="small" />
                ) : (
                  <ul>
                    <li>{t('admin:users.impactAthletes', 'Assigned Athletes')}: <strong>{coachImpact?.assignedAthletesCount || 0}</strong></li>
                    <li>{t('admin:users.impactWorkoutPlans', 'Active Workout Templates')}: <strong>{coachImpact?.activeWorkoutPlansCount || 0}</strong></li>
                    <li>{t('admin:users.impactNutritionPlans', 'Active Nutrition Assignments')}: <strong>{coachImpact?.activeNutritionPlansCount || 0}</strong></li>
                  </ul>
                )}
              </div>
            )}

            <Form form={form} layout="vertical" className="deactivation-form">
              {selectedUser.role === 'Coach' && coachImpact && coachImpact.assignedAthletesCount > 0 && (
                <Form.Item
                  name="reassignCoachId"
                  label={t('admin:users.reassignCoachLabel', 'Reassign Assigned Athletes To (Optional)')}
                >
                  <Select placeholder={t('admin:users.reassignPlaceholder', 'Select new coach (or leave unassigned)')} allowClear>
                    {availableCoaches
                      .filter((c) => c.id !== selectedUser.id)
                      .map((c) => (
                        <Option key={c.id} value={c.id}>
                          {c.name}
                        </Option>
                      ))}
                  </Select>
                </Form.Item>
              )}

              <Form.Item
                name="reasonPreset"
                label={t('admin:users.deactivationReasonLabel', 'Deactivation Reason')}
                rules={[{ required: true, message: t('admin:users.reasonRequired', 'Please select a deactivation reason.') }]}
              >
                <Select placeholder={t('admin:users.selectReason', 'Select a reason')}>
                  {DEACTIVATION_REASONS.map((r) => (
                    <Option key={r.value} value={r.value}>
                      {t(r.labelKey, r.fallback)}
                    </Option>
                  ))}
                </Select>
              </Form.Item>

              <Form.Item
                noStyle
                shouldUpdate={(prevValues, currentValues) => prevValues.reasonPreset !== currentValues.reasonPreset}
              >
                {({ getFieldValue }) =>
                  getFieldValue('reasonPreset') === 'Custom Reason' ? (
                    <Form.Item
                      name="customReason"
                      label={t('admin:users.customReasonLabel', 'Custom Notes / Explanation')}
                      rules={[{ required: true, message: t('admin:users.customReasonRequired', 'Please enter custom reason notes.') }]}
                    >
                      <Input.TextArea rows={3} placeholder={t('admin:users.customReasonPlaceholder', 'Enter detailed reason for suspension...')} />
                    </Form.Item>
                  ) : null
                }
              </Form.Item>
            </Form>
          </div>
        )}
      </Modal>

      {/* Login Audit Drawer */}
      <Drawer
        title={auditUser ? `${t('admin:users.auditDrawerTitle', 'Sign-In History')} - ${auditUser.fullName}` : 'Sign-In History'}
        placement={isRTL ? 'left' : 'right'}
        width={520}
        onClose={() => setAuditDrawerVisible(false)}
        open={auditDrawerVisible}
      >
        {loadingAudit ? (
          <div className="drawer-loading"><Spin size="large" /></div>
        ) : (
          <div className="audit-timeline">
            {auditLogs.length === 0 ? (
              <p className="text-muted">{t('admin:users.noAuditLogs', 'No sign-in records found for this user.')}</p>
            ) : (
              auditLogs.map((log) => (
                <div key={log.id} className="audit-item">
                  <div className="audit-header">
                    <span className="audit-action">{log.action}</span>
                    <span className="audit-date">{new Date(log.createdAt).toLocaleString()}</span>
                  </div>
                  <div className="audit-details">
                    <div>{t('admin:users.performedBy', 'Performed By')}: {log.performedByName || 'System'}</div>
                    {log.ipAddress && <div>IP: <code>{log.ipAddress}</code></div>}
                    {log.details && <div className="audit-notes">{log.details}</div>}
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </Drawer>
    </div>
  );
};

export default UserManagement;
