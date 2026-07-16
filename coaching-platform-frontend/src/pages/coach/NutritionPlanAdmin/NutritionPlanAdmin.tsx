import React, { useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Empty,
  Form,
  Input,
  Modal,
  Pagination,
  Popconfirm,
  Select,
  Skeleton,
  Space,
  Table,
  Tag,
  Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useGetRoster } from '../../../hooks/useCoachHub/useCoachHub';
import {
  useAssignNutritionPlan,
  useChangeNutritionPlanStatus,
  useNutritionPlans,
} from '../../../hooks/useNutritionPlans/useNutritionPlans';
import { validateNutritionPlan } from '../../../api/nutritionPlan';
import type {
  ContentStatus,
  NutritionPlanSummary,
  NutritionPlanValidation,
} from '../../../types/NutritionPlan';
import './NutritionPlanAdmin.scss';

const { TextArea } = Input;
const { Title, Text } = Typography;

const statusColor: Record<ContentStatus, string> = {
  Draft: 'default',
  InReview: 'orange',
  Published: 'green',
  Archived: 'red',
};

const nextStatus: Partial<Record<ContentStatus, ContentStatus>> = {
  Draft: 'InReview',
  InReview: 'Published',
  Published: 'Archived',
  Archived: 'Draft',
};

const NutritionPlanAdmin: React.FC = () => {
  const { i18n } = useTranslation();
  const ar = i18n.resolvedLanguage === 'ar';
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<ContentStatus | undefined>();
  const [validation, setValidation] = useState<NutritionPlanValidation | null>(null);
  const [validationOpen, setValidationOpen] = useState(false);
  const [assignmentPlan, setAssignmentPlan] = useState<NutritionPlanSummary | null>(null);
  const [athleteIds, setAthleteIds] = useState<number[]>([]);
  const [assignmentNotes, setAssignmentNotes] = useState('');

  const copy = ar ? {
    title: 'خطط التغذية', subtitle: 'راجع وعدّل وانشر خطط التغذية الخاصة بالمدرب ثم عيّنها للمتدربين.',
    newPlan: 'خطة جديدة', search: 'ابحث باسم الخطة', allStatuses: 'كل الحالات', edit: 'تعديل',
    validate: 'فحص', assign: 'تعيين', name: 'الاسم', calories: 'السعرات', meals: 'الوجبات',
    assignments: 'التعيينات', status: 'الحالة', actions: 'الإجراءات', save: 'حفظ الخطة', cancel: 'إلغاء',
    details: 'تفاصيل الخطة', targetCalories: 'السعرات المستهدفة',
    validationTitle: 'فحص جاهزية النشر', valid: 'الخطة جاهزة للنشر.', invalid: 'يجب حل هذه المشاكل قبل النشر.',
    assignTitle: 'تعيين خطة التغذية', selectAthletes: 'اختر المتدربين', notes: 'ملاحظات للمتدرب', confirmAssign: 'تعيين',
    noPlans: 'لا توجد خطط تغذية.', version: 'الإصدار', next: 'نقل إلى', remove: 'حذف', training: 'تدريب', rest: 'راحة',
  } : {
    title: 'Nutrition plans', subtitle: 'Review, edit, publish, and assign coach nutrition plans.',
    newPlan: 'New plan', search: 'Search plan name', allStatuses: 'All statuses', edit: 'Edit', validate: 'Validate', assign: 'Assign',
    name: 'Name', calories: 'Calories', meals: 'Meals', assignments: 'Assignments', status: 'Status', actions: 'Actions',
    save: 'Save plan', cancel: 'Cancel', details: 'Plan details', targetCalories: 'Target calories',
    validationTitle: 'Publish readiness', valid: 'This plan is ready to publish.', invalid: 'Resolve these issues before publishing.',
    assignTitle: 'Assign nutrition plan', selectAthletes: 'Select athletes', notes: 'Notes for athlete', confirmAssign: 'Assign',
    noPlans: 'No nutrition plans found.', version: 'Version', next: 'Move to', remove: 'Remove', training: 'Training', rest: 'Rest',
  };

  const { data, isLoading } = useNutritionPlans({ page, pageSize: 10, search: search || undefined, status });
  const { data: roster } = useGetRoster(1, 100);
  const statusMutation = useChangeNutritionPlanStatus();
  const assignMutation = useAssignNutritionPlan();

  const showValidation = async (id: number) => {
    setValidation(await validateNutritionPlan(id));
    setValidationOpen(true);
  };

  const columns: ColumnsType<NutritionPlanSummary> = useMemo(() => [
    {
      title: copy.name,
      render: (_, plan) => (
        <div>
          <strong>{plan.name}</strong>
          <small className="nutrition-admin__version">{copy.version} {plan.contentVersion}</small>
        </div>
      ),
    },
    {
      title: copy.calories,
      dataIndex: 'targetCalories',
      render: (value, plan) => {
        const conditional = plan.trainingDayCalories !== plan.restDayCalories;
        const mismatch = value !== plan.trainingDayCalories || value !== plan.restDayCalories;
        return (
          <span className={mismatch ? 'nutrition-admin__mismatch' : ''}>
            {conditional
              ? `${value} · ${copy.training} ${plan.trainingDayCalories} / ${copy.rest} ${plan.restDayCalories}`
              : `${value} / ${plan.mealBlockCalories}`}
          </span>
        );
      },
    },
    { title: copy.meals, dataIndex: 'mealBlockCount' },
    { title: copy.assignments, dataIndex: 'activeAssignmentCount' },
    { title: copy.status, dataIndex: 'contentStatus', render: (value: ContentStatus) => <Tag color={statusColor[value]}>{value}</Tag> },
    {
      title: copy.actions,
      render: (_, plan) => (
        <Space wrap>
          <Button size="small" onClick={() => navigate(`/coach/nutrition-plans/edit/${plan.id}`)}>{copy.edit}</Button>
          <Button size="small" onClick={() => showValidation(plan.id)}>{copy.validate}</Button>
          {plan.contentStatus === 'Published' && (
            <Button size="small" type="primary" onClick={() => setAssignmentPlan(plan)}>{copy.assign}</Button>
          )}
          {nextStatus[plan.contentStatus] && (
            <Popconfirm
              title={`${copy.next} ${nextStatus[plan.contentStatus]}?`}
              onConfirm={() =>
                statusMutation.mutate({
                  id: plan.id,
                  status: nextStatus[plan.contentStatus]!,
                  expectedContentVersion: plan.contentVersion,
                })
              }
            >
              <Button size="small">{nextStatus[plan.contentStatus]}</Button>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ], [ar, copy, statusMutation, navigate]);

  return (
    <div className="nutrition-admin">
      <header className="nutrition-admin__header">
        <div>
          <Title level={2}>{copy.title}</Title>
          <Text type="secondary">{copy.subtitle}</Text>
        </div>
        <Button
          type="primary"
          size="large"
          icon={<span className="material-symbols-outlined">add</span>}
          onClick={() => navigate('/coach/nutrition-plans/new')}
        >
          {copy.newPlan}
        </Button>
      </header>

      <Card className="nutrition-admin__filters">
        <Space wrap>
          <Input.Search allowClear placeholder={copy.search} onSearch={(value) => { setSearch(value); setPage(1); }} />
          <Select
            allowClear
            placeholder={copy.allStatuses}
            value={status}
            onChange={(value) => { setStatus(value); setPage(1); }}
            options={Object.keys(statusColor).map(value => ({ value, label: value }))}
          />
        </Space>
      </Card>

      <Card className="nutrition-admin__table">
        {isLoading ? (
          <Skeleton active />
        ) : (
          <Table
            rowKey="id"
            columns={columns}
            dataSource={data?.items}
            pagination={false}
            locale={{ emptyText: <Empty description={copy.noPlans} /> }}
            scroll={{ x: 850 }}
          />
        )}
        {!!data?.totalCount && (
          <Pagination current={page} pageSize={10} total={data.totalCount} onChange={setPage} showSizeChanger={false} />
        )}
      </Card>

      <Modal open={validationOpen} title={copy.validationTitle} footer={null} onCancel={() => setValidationOpen(false)}>
        {validation && (
          <>
            <Alert
              type={validation.isValidForPublish ? 'success' : 'error'}
              showIcon
              message={validation.isValidForPublish ? copy.valid : copy.invalid}
              description={`${copy.training}: ${validation.trainingDayCalories} kcal · ${copy.rest}: ${validation.restDayCalories} kcal · ${copy.targetCalories}: ${validation.targetCalories} kcal`}
            />
            <div className="nutrition-admin__issues">
              {validation.issues.map((issue, index) => (
                <Alert key={`${issue.code}-${index}`} type="error" showIcon message={issue.message} description={issue.path} />
              ))}
            </div>
          </>
        )}
      </Modal>

      <Modal
        open={!!assignmentPlan}
        title={copy.assignTitle}
        confirmLoading={assignMutation.isPending}
        okText={copy.confirmAssign}
        onCancel={() => setAssignmentPlan(null)}
        onOk={async () => {
          if (!assignmentPlan || !athleteIds.length) return;
          await assignMutation.mutateAsync({ id: assignmentPlan.id, athleteIds, notes: assignmentNotes || undefined });
          setAssignmentPlan(null);
          setAthleteIds([]);
          setAssignmentNotes('');
        }}
      >
        <Form layout="vertical">
          <Form.Item label={copy.selectAthletes} required>
            <Select
              mode="multiple"
              value={athleteIds}
              onChange={setAthleteIds}
              options={roster?.items.map(item => ({ value: item.athleteId, label: item.athleteName }))}
            />
          </Form.Item>
          <Form.Item label={copy.notes}>
            <TextArea value={assignmentNotes} onChange={event => setAssignmentNotes(event.target.value)} rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default NutritionPlanAdmin;
