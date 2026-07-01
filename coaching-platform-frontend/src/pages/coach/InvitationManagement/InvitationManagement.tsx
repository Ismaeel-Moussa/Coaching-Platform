import React, { useState } from 'react';
import { Table, Button, Modal, Form, Input, Select, InputNumber, Space, Badge, Tooltip } from 'antd';
import {
  useGetInvitations,
  useCreateInvitation,
  useResendInvitation,
  useRevokeInvitation,
} from '../../../hooks/useInvitations/useInvitations';
import { InvitationStatus, InvitationStatusLabel } from '../../../types/Invitation';
import type { InvitationDto, InvitationRole } from '../../../types/Invitation';
import './InvitationManagement.scss';

const ROLE_OPTIONS: InvitationRole[] = ['Athlete', 'Coach', 'Admin'];

const STATUS_BADGES: Record<number, { status: 'default' | 'success' | 'processing' | 'error' | 'warning'; text: string }> = {
  [InvitationStatus.Pending]: { status: 'warning', text: 'Pending' },
  [InvitationStatus.Accepted]: { status: 'success', text: 'Accepted' },
  [InvitationStatus.Expired]: { status: 'default', text: 'Expired' },
  [InvitationStatus.Revoked]: { status: 'error', text: 'Revoked' },
};

const InvitationManagement: React.FC = () => {
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
      title: 'Link Copied',
      content: 'Invitation link has been copied to your clipboard.',
    });
  };

  const columns = [
    {
      title: 'Invitee Email',
      dataIndex: 'email',
      key: 'email',
      render: (text: string) => <strong className="invite-table__email">{text}</strong>,
    },
    {
      title: 'Assigned Role',
      dataIndex: 'role',
      key: 'role',
      render: (role: string) => (
        <span className={`invite-table__role-badge invite-table__role-badge--${role.toLowerCase()} font-data`}>
          {role}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: number) => {
        const badgeInfo = STATUS_BADGES[status] || { status: 'default', text: 'Unknown' };
        return <Badge status={badgeInfo.status} text={badgeInfo.text} className="mono" />;
      },
    },
    {
      title: 'Expires At',
      dataIndex: 'expiresAt',
      key: 'expiresAt',
      render: (dateStr: string) => <span className="mono">{new Date(dateStr).toLocaleString()}</span>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: InvitationDto) => {
        const isPending = record.status === InvitationStatus.Pending;
        const isExpired = record.status === InvitationStatus.Expired;
        const isRevoked = record.status === InvitationStatus.Revoked;

        return (
          <Space size="middle">
            <Tooltip title="Copy Invite Link">
              <Button
                type="text"
                disabled={!isPending}
                onClick={() => handleCopyLink(record.inviteUrl)}
                icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>content_copy</span>}
              />
            </Tooltip>

            <Tooltip title="Resend Email Invitation">
              <Button
                type="text"
                disabled={record.status === InvitationStatus.Accepted}
                onClick={() => handleResend(record.id)}
                loading={resendMutation.isPending && resendMutation.variables === record.id}
                icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>send</span>}
              />
            </Tooltip>

            <Tooltip title="Revoke Invitation">
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
          <h1 className="invitation-management__title">Invitation Management</h1>
          <p className="invitation-management__subtitle">Issue, resend, and revoke client/staff onboarding invitations</p>
        </div>
        <Button
          type="primary"
          onClick={handleOpenModal}
          icon={<span className="material-symbols-outlined">person_add</span>}
          className="invitation-management__add-btn"
        >
          Invite Member
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
        className="invitation-management__table"
      />

      <Modal
        title="Invite New Member"
        open={isInviteModalVisible}
        onCancel={() => setIsInviteModalVisible(false)}
        onOk={handleSendInvite}
        okText="Send Invitation"
        okButtonProps={{ loading: createMutation.isPending }}
        width={450}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="email"
            label="Email Address"
            rules={[
              { required: true, message: 'Please enter email address' },
              { type: 'email', message: 'Please enter a valid email address' },
            ]}
          >
            <Input placeholder="e.g. recruit@jokernutrition.com" />
          </Form.Item>

          <Form.Item
            name="role"
            label="Onboarding Role"
            rules={[{ required: true, message: 'Please select role' }]}
          >
            <Select placeholder="Select role">
              {ROLE_OPTIONS.map((r) => (
                <Select.Option key={r} value={r}>
                  {r}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="expiryHours"
            label="Link Expiry (Hours)"
            help="The link will automatically expire after this period."
          >
            <InputNumber min={1} max={720} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default InvitationManagement;
