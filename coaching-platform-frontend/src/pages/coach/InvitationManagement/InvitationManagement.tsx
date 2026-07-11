import React, { useState } from 'react';
import { Table, Button, Modal, Form, Input, Select, InputNumber, Space, Badge, Tooltip } from 'antd';
import { useTranslation } from 'react-i18next';
import {
  useGetInvitations,
  useCreateInvitation,
  useResendInvitation,
  useRevokeInvitation,
} from '../../../hooks/useInvitations/useInvitations';
import { InvitationStatus } from '../../../types/Invitation';
import type { InvitationDto, InvitationRole, InvitationStatusValue } from '../../../types/Invitation';
import './InvitationManagement.scss';

const ROLE_OPTIONS: InvitationRole[] = ['Athlete', 'Coach', 'Admin'];

const InvitationManagement: React.FC = () => {
  const { t, i18n } = useTranslation(['common', 'coach']);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [isInviteModalVisible, setIsInviteModalVisible] = useState<boolean>(false);

  const [form] = Form.useForm();

  // Queries
  const { data, isLoading } = useGetInvitations(currentPage, pageSize);

  // Mutations
  const createMutation = useCreateInvitation();
  const resendMutation = useResendInvitation();
  const revokeMutation = useRevokeInvitation();

  const handleOpenModal = () => {
    form.resetFields();
    form.setFieldsValue({ expiryHours: 72 }); // default 72h
    setIsInviteModalVisible(true);
  };

  const handleSendInvite = async () => {
    try {
      const values = await form.validateFields();
      await createMutation.mutateAsync({
        email: values.email,
        role: values.role,
        expiryHours: values.expiryHours,
      });
      setIsInviteModalVisible(false);
    } catch (err) {
      // Handled by validation/hook
    }
  };

  const handleResend = (id: number) => {
    resendMutation.mutate(id);
  };

  const handleRevoke = (id: number) => {
    revokeMutation.mutate(id);
  };

  const handleCopyLink = (url: string) => {
    navigator.clipboard.writeText(url);
    Modal.success({
      title: t('coach:invitations.linkCopied'),
      content: t('coach:invitations.linkCopiedDesc'),
    });
  };

  const getStatusBadge = (status: InvitationStatusValue) => {
    switch (status) {
      case InvitationStatus.Pending:
      case 0:
        return <Badge status="warning" text={t('common:status.pending')} className="mono" />;
      case InvitationStatus.Accepted:
      case 1:
        return <Badge status="success" text={t('common:status.accepted')} className="mono" />;
      case InvitationStatus.Expired:
      case 2:
        return <Badge status="default" text={t('common:status.expired')} className="mono" />;
      case InvitationStatus.Revoked:
      case 3:
        return <Badge status="error" text={t('common:status.revoked')} className="mono" />;
      default:
        return <Badge status="default" text={t('common:status.unknown')} className="mono" />;
    }
  };

  const columns = [
    {
      title: t('coach:invitations.inviteEmail'),
      dataIndex: 'email',
      key: 'email',
      render: (text: string) => <strong className="invite-table__email">{text}</strong>,
    },
    {
      title: t('coach:invitations.assignedRole'),
      dataIndex: 'role',
      key: 'role',
      render: (role: string) => (
        <span className={`invite-table__role-badge invite-table__role-badge--${role.toLowerCase()} font-data`}>
          {role}
        </span>
      ),
    },
    {
      title: t('coach:invitations.status'),
      dataIndex: 'status',
      key: 'status',
      render: (status: InvitationStatusValue) => getStatusBadge(status),
    },
    {
      title: t('coach:invitations.expiresAt'),
      dataIndex: 'expiresAt',
      key: 'expiresAt',
      render: (dateStr: string) => <span className="mono">{new Date(dateStr).toLocaleString(i18n.language)}</span>,
    },
    {
      title: t('coach:invitations.actions'),
      key: 'actions',
      render: (_: any, record: InvitationDto) => {
        const isPending = record.status === InvitationStatus.Pending || record.status === 0;
        const isExpired = record.status === InvitationStatus.Expired || record.status === 2;
        const isRevoked = record.status === InvitationStatus.Revoked || record.status === 3;

        return (
          <Space size="middle">
            <Tooltip title={t('coach:invitations.copyLink')}>
              <Button
                type="text"
                disabled={!isPending}
                onClick={() => handleCopyLink(record.inviteUrl)}
                icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>content_copy</span>}
              />
            </Tooltip>

            <Tooltip title={t('coach:invitations.resend')}>
              <Button
                type="text"
                disabled={record.status === InvitationStatus.Accepted || record.status === 1}
                onClick={() => handleResend(record.id)}
                loading={resendMutation.isPending && resendMutation.variables === record.id}
                icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>send</span>}
              />
            </Tooltip>

            <Tooltip title={t('coach:invitations.revoke')}>
              <Button
                type="text"
                danger
                disabled={!isPending && !isExpired}
                onClick={() => handleRevoke(record.id)}
                loading={revokeMutation.isPending && revokeMutation.variables === record.id}
                icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>cancel</span>}
              />
            </Tooltip>
          </Space>
        );
      },
    },
  ];

  return (
    <div id="invitation-management-page" className="invitation-management animate-fade-in">
      <div className="invitation-management__header">
        <div>
          <h1 className="invitation-management__title">{t('coach:invitations.title')}</h1>
          <p className="invitation-management__subtitle">{t('coach:invitations.subtitle')}</p>
        </div>
        <Button
          type="primary"
          onClick={handleOpenModal}
          icon={<span className="material-symbols-outlined">person_add</span>}
          className="invitation-management__add-btn"
        >
          {t('coach:invitations.inviteMember')}
        </Button>
      </div>

      <Table
        dataSource={data?.items || []}
        columns={columns}
        rowKey="id"
        loading={isLoading}
        pagination={{
          current: currentPage,
          pageSize: pageSize,
          total: data?.totalCount || 0,
          onChange: (page, size) => {
            setCurrentPage(page);
            setPageSize(size);
          },
        }}
        className="invitation-management__table invitation-management__desktop-table"
      />
      <div className="invitation-management__mobile-cards">
        {data?.items.map((record) => {
          const isPending = record.status === InvitationStatus.Pending || record.status === 0;
          const isExpired = record.status === InvitationStatus.Expired || record.status === 2;
          const isRevoked = record.status === InvitationStatus.Revoked || record.status === 3;

          return (
            <div key={record.id} className="invitation-management__card-item">
              <div className="invitation-management__card-header">
                <strong className="invite-email">{record.email}</strong>
                <span className={`invite-role-badge invite-role-badge--${record.role.toLowerCase()} font-data`}>
                  {record.role}
                </span>
              </div>
              <div className="invitation-management__card-body">
                <div className="invitation-management__card-row">
                  <span className="label">{t('coach:invitations.status')}</span>
                  <span className="value">
                    {getStatusBadge(record.status)}
                  </span>
                </div>
                <div className="invitation-management__card-row">
                  <span className="label">{t('coach:invitations.expiresAt')}</span>
                  <span className="value mono">{new Date(record.expiresAt).toLocaleString(i18n.language)}</span>
                </div>
              </div>
              <div className="invitation-management__card-footer">
                <Space size="middle">
                  <Button
                    size="small"
                    disabled={!isPending}
                    onClick={() => handleCopyLink(record.inviteUrl)}
                    icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>content_copy</span>}
                  >
                    {t('coach:invitations.copyLink')}
                  </Button>
                  <Button
                    size="small"
                    disabled={record.status === InvitationStatus.Accepted || record.status === 1}
                    onClick={() => handleResend(record.id)}
                    loading={resendMutation.isPending && resendMutation.variables === record.id}
                    icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>send</span>}
                  >
                    {t('coach:invitations.resend')}
                  </Button>
                  <Button
                    danger
                    size="small"
                    disabled={!isPending && !isExpired}
                    onClick={() => handleRevoke(record.id)}
                    loading={revokeMutation.isPending && revokeMutation.variables === record.id}
                    icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>cancel</span>}
                  >
                    {t('coach:invitations.revoke')}
                  </Button>
                </Space>
              </div>
            </div>
          );
        })}
        {/* Mobile pagination controls */}
        {data && data.totalCount > pageSize && (
          <div className="invitation-management__mobile-pagination">
            <Button
              disabled={currentPage === 1}
              onClick={() => setCurrentPage((p) => p - 1)}
              size="small"
            >
              {t('common:pagination.prev')}
            </Button>
            <span className="pagination-text">
              {t('common:pagination.pageOf', { page: currentPage, total: Math.ceil(data.totalCount / pageSize) })}
            </span>
            <Button
              disabled={currentPage * pageSize >= data.totalCount}
              onClick={() => setCurrentPage((p) => p + 1)}
              size="small"
            >
              {t('common:pagination.next')}
            </Button>
          </div>
        )}
      </div>

      <Modal
        title={t('coach:invitations.inviteTitle')}
        open={isInviteModalVisible}
        onCancel={() => setIsInviteModalVisible(false)}
        onOk={handleSendInvite}
        okText={t('coach:invitations.sendInvite')}
        okButtonProps={{ loading: createMutation.isPending }}
        width={450}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="email"
            label={t('coach:invitations.emailLabel')}
            rules={[
              { required: true, message: t('coach:invitations.enterEmail', { defaultValue: 'Please enter email address' }) },
              { type: 'email', message: t('coach:invitations.validEmail', { defaultValue: 'Please enter a valid email address' }) },
            ]}
          >
            <Input placeholder={t('coach:invitations.emailPlaceholder')} />
          </Form.Item>

          <Form.Item
            name="role"
            label={t('coach:invitations.roleLabel')}
            rules={[{ required: true, message: t('coach:invitations.selectRole', { defaultValue: 'Please select role' }) }]}
          >
            <Select placeholder={t('coach:invitations.roleSelect')}>
              {ROLE_OPTIONS.map((r) => (
                <Select.Option key={r} value={r}>
                  {r}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="expiryHours"
            label={t('coach:invitations.expiryLabel')}
            help={t('coach:invitations.expiryHelp')}
          >
            <InputNumber min={1} max={720} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default InvitationManagement;
