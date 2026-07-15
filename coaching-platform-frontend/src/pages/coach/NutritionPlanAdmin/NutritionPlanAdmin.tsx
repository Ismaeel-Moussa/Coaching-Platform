import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Checkbox,
  Empty,
  Form,
  Input,
  InputNumber,
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
import { useInfiniteQuery } from '@tanstack/react-query';
import { useGetRoster } from '../../../hooks/useCoachHub/useCoachHub';
import { searchFoods } from '../../../api/food';
import { getRecipes } from '../../../api/recipe';
import {
  useAssignNutritionPlan,
  useChangeNutritionPlanStatus,
  useNutritionPlan,
  useNutritionPlans,
  useSaveNutritionPlan,
} from '../../../hooks/useNutritionPlans/useNutritionPlans';
import { validateNutritionPlan } from '../../../api/nutritionPlan';
import type {
  ContentStatus,
  NutritionPlanForm,
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

const initialPlan: NutritionPlanForm = {
  name: '',
  nameAr: '',
  description: '',
  descriptionAr: '',
  targetCalories: 2000,
  minimumProteinGrams: 150,
  mealBlocks: [{
    mealType: 'Breakfast',
    label: 'Meal 1',
    labelAr: 'الوجبة 1',
    targetCalories: 2000,
    trainingDayOnly: false,
    restDayOnly: false,
    instructions: '',
    instructionsAr: '',
    options: [{
      label: 'Option 1',
      labelAr: 'الخيار 1',
      isCompleteOption: true,
      items: [{
        itemName: '', itemNameAr: '', quantity: 1, unit: 'Piece', measurementState: 'Unspecified',
      }],
    }],
  }],
  rules: [],
};

const copyPlanForForm = (plan: NutritionPlanForm): NutritionPlanForm => JSON.parse(JSON.stringify(plan));

const NutritionPlanAdmin: React.FC = () => {
  const { i18n } = useTranslation();
  const ar = i18n.resolvedLanguage === 'ar';
  const [form] = Form.useForm<NutritionPlanForm>();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<ContentStatus | undefined>();
  const [editingId, setEditingId] = useState<number | undefined>();
  const [editorOpen, setEditorOpen] = useState(false);
  const [validation, setValidation] = useState<NutritionPlanValidation | null>(null);
  const [validationOpen, setValidationOpen] = useState(false);
  const [assignmentPlan, setAssignmentPlan] = useState<NutritionPlanSummary | null>(null);
  const [athleteIds, setAthleteIds] = useState<number[]>([]);
  const [assignmentNotes, setAssignmentNotes] = useState('');
  const [foodSearchInput, setFoodSearchInput] = useState('');
  const [recipeSearchInput, setRecipeSearchInput] = useState('');
  const [foodSearch, setFoodSearch] = useState('');
  const [recipeSearch, setRecipeSearch] = useState('');

  const copy = ar ? {
    title: 'خطط التغذية', subtitle: 'راجع وعدّل وانشر خطط المدرب ثم عيّنها للمتدربين.',
    newPlan: 'خطة جديدة', search: 'ابحث باسم الخطة', allStatuses: 'كل الحالات', edit: 'تعديل',
    validate: 'فحص', assign: 'تعيين', name: 'الاسم', calories: 'السعرات', meals: 'الوجبات',
    assignments: 'التعيينات', status: 'الحالة', actions: 'الإجراءات', save: 'حفظ الخطة', cancel: 'إلغاء',
    details: 'تفاصيل الخطة', nameEn: 'الاسم بالإنجليزية', nameAr: 'الاسم بالعربية', descEn: 'الوصف بالإنجليزية',
    descAr: 'الوصف بالعربية', targetCalories: 'السعرات المستهدفة', minProtein: 'الحد الأدنى للبروتين (غ)',
    mealBlocks: 'مجموعات الوجبات', addMeal: 'إضافة وجبة', mealType: 'نوع الوجبة', labelEn: 'العنوان بالإنجليزية',
    labelAr: 'العنوان بالعربية', mealCalories: 'سعرات الوجبة', trainingOnly: 'يوم التدريب فقط', restOnly: 'يوم الراحة فقط',
    instructionsEn: 'التعليمات بالإنجليزية', instructionsAr: 'التعليمات بالعربية', options: 'الخيارات', addOption: 'إضافة خيار',
    completeOption: 'خيار كامل', items: 'العناصر', addItem: 'إضافة عنصر', food: 'طعام', recipe: 'وصفة',
    customEn: 'عنصر مخصص بالإنجليزية', customAr: 'عنصر مخصص بالعربية', quantity: 'الكمية', unit: 'الوحدة', preparation: 'حالة التحضير', alternativeGroup: 'مجموعة البدائل', training: 'تدريب', rest: 'راحة',
    rules: 'القواعد', addRule: 'إضافة قاعدة', ruleType: 'نوع القاعدة', ruleEn: 'القاعدة بالإنجليزية', ruleAr: 'القاعدة بالعربية',
    validationTitle: 'فحص جاهزية النشر', valid: 'الخطة جاهزة للنشر.', invalid: 'يجب حل هذه المشاكل قبل النشر.',
    assignTitle: 'تعيين خطة التغذية', selectAthletes: 'اختر المتدربين', notes: 'ملاحظات للمتدرب', confirmAssign: 'تعيين',
    noPlans: 'لا توجد خطط تغذية.', version: 'الإصدار', next: 'نقل إلى', remove: 'حذف', sourceHint: 'اختر طعاماً أو وصفة، أو اكتب عنصراً مخصصاً فقط.',
  } : {
    title: 'Nutrition plans', subtitle: 'Review, edit, publish, and assign coach nutrition plans.',
    newPlan: 'New plan', search: 'Search plan name', allStatuses: 'All statuses', edit: 'Edit', validate: 'Validate', assign: 'Assign',
    name: 'Name', calories: 'Calories', meals: 'Meals', assignments: 'Assignments', status: 'Status', actions: 'Actions',
    save: 'Save plan', cancel: 'Cancel', details: 'Plan details', nameEn: 'English name', nameAr: 'Arabic name',
    descEn: 'English description', descAr: 'Arabic description', targetCalories: 'Target calories', minProtein: 'Minimum protein (g)',
    mealBlocks: 'Meal blocks', addMeal: 'Add meal', mealType: 'Meal type', labelEn: 'English label', labelAr: 'Arabic label',
    mealCalories: 'Meal calories', trainingOnly: 'Training day only', restOnly: 'Rest day only', instructionsEn: 'English instructions',
    instructionsAr: 'Arabic instructions', options: 'Options', addOption: 'Add option', completeOption: 'Complete option', items: 'Items',
    addItem: 'Add item', food: 'Food', recipe: 'Recipe', customEn: 'English custom item', customAr: 'Arabic custom item', quantity: 'Quantity',
    unit: 'Unit', preparation: 'Preparation', alternativeGroup: 'Alternative group', training: 'Training', rest: 'Rest', rules: 'Rules', addRule: 'Add rule', ruleType: 'Rule type', ruleEn: 'English rule', ruleAr: 'Arabic rule',
    validationTitle: 'Publish readiness', valid: 'This plan is ready to publish.', invalid: 'Resolve these issues before publishing.',
    assignTitle: 'Assign nutrition plan', selectAthletes: 'Select athletes', notes: 'Notes for athlete', confirmAssign: 'Assign',
    noPlans: 'No nutrition plans found.', version: 'Version', next: 'Move to', remove: 'Remove', sourceHint: 'Choose a food or recipe, or enter one custom item only.',
  };

  const { data, isLoading } = useNutritionPlans({ page, pageSize: 10, search: search || undefined, status });
  const { data: editedPlan, isFetching: editorLoading } = useNutritionPlan(editingId);
  const { data: roster } = useGetRoster(1, 100);
  const foodsQuery = useInfiniteQuery({
    queryKey: ['nutrition-plan-food-options', foodSearch],
    queryFn: ({ pageParam }) => searchFoods({ search: foodSearch || undefined, page: pageParam, pageSize: 50 }),
    initialPageParam: 1,
    getNextPageParam: lastPage => lastPage.hasNextPage ? lastPage.page + 1 : undefined,
  });
  const recipesQuery = useInfiniteQuery({
    queryKey: ['nutrition-plan-recipe-options', recipeSearch],
    queryFn: ({ pageParam }) => getRecipes({ isJokerRecipe: true, search: recipeSearch || undefined, page: pageParam, pageSize: 50 }),
    initialPageParam: 1,
    getNextPageParam: lastPage => lastPage.hasNextPage ? lastPage.page + 1 : undefined,
  });
  const saveMutation = useSaveNutritionPlan();
  const statusMutation = useChangeNutritionPlanStatus();
  const assignMutation = useAssignNutritionPlan();

  useEffect(() => {
    if (editedPlan && editorOpen) form.setFieldsValue({
      ...copyPlanForForm(editedPlan),
      expectedContentVersion: editedPlan.contentVersion,
    });
  }, [editedPlan, editorOpen, form]);

  useEffect(() => {
    const timer = window.setTimeout(() => setFoodSearch(foodSearchInput.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [foodSearchInput]);

  useEffect(() => {
    const timer = window.setTimeout(() => setRecipeSearch(recipeSearchInput.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [recipeSearchInput]);

  const mealTypes = ['Breakfast', 'Lunch', 'Dinner', 'Snack', 'Suhoor', 'Iftar', 'PreWorkout', 'PostWorkout'];
  const units = ['Gram', 'Milliliter', 'Piece', 'Tablespoon', 'Teaspoon', 'Cup', 'Scoop'];
  const preparationStates = ['Unspecified', 'Raw', 'Cooked', 'Drained'];

  const foodOptions = useMemo(() => {
    const options = new Map<number, string>();
    editedPlan?.mealBlocks.flatMap(block => block.options).flatMap(option => option.items)
      .filter(item => item.foodId && item.foodName)
      .forEach(item => options.set(item.foodId!, ar ? item.foodNameAr || item.foodName! : item.foodName!));
    foodsQuery.data?.pages.flatMap(result => result.items)
      .forEach(food => options.set(food.id, ar ? food.nameAr || food.name : food.name));
    return Array.from(options, ([value, label]) => ({ value, label }));
  }, [ar, editedPlan, foodsQuery.data]);

  const recipeOptions = useMemo(() => {
    const options = new Map<number, string>();
    editedPlan?.mealBlocks.flatMap(block => block.options).flatMap(option => option.items)
      .filter(item => item.recipeId && item.recipeName)
      .forEach(item => options.set(item.recipeId!, ar ? item.recipeNameAr || item.recipeName! : item.recipeName!));
    recipesQuery.data?.pages.flatMap(result => result.items)
      .forEach(recipe => options.set(recipe.id, ar ? recipe.nameAr || recipe.name : recipe.name));
    return Array.from(options, ([value, label]) => ({ value, label }));
  }, [ar, editedPlan, recipesQuery.data]);

  const loadMoreOptions = (
    event: React.UIEvent<HTMLDivElement>,
    hasNextPage: boolean | undefined,
    isFetchingNextPage: boolean,
    fetchNextPage: () => unknown,
  ) => {
    const target = event.currentTarget;
    if (hasNextPage && !isFetchingNextPage && target.scrollTop + target.offsetHeight >= target.scrollHeight - 24)
      fetchNextPage();
  };

  const setNestedField = (path: Array<string | number>, value: unknown) =>
    form.setFieldValue(path as never, value);

  const openNew = () => {
    setEditingId(undefined);
    form.resetFields();
    form.setFieldsValue(copyPlanForForm(initialPlan));
    setEditorOpen(true);
  };

  const openEdit = (id: number) => {
    setEditingId(id);
    setEditorOpen(true);
  };

  const save = async () => {
    const values = await form.validateFields();
    const saved = await saveMutation.mutateAsync({ id: editingId, form: values });
    setEditingId(saved.id);
    setEditorOpen(false);
  };

  const showValidation = async (id: number) => {
    setValidation(await validateNutritionPlan(id));
    setValidationOpen(true);
  };

  const columns: ColumnsType<NutritionPlanSummary> = useMemo(() => [
    {
      title: copy.name,
      render: (_, plan) => <div><strong>{ar ? plan.nameAr || plan.name : plan.name}</strong><small className="nutrition-admin__version">{copy.version} {plan.contentVersion}</small></div>,
    },
    { title: copy.calories, dataIndex: 'targetCalories', render: (value, plan) => {
      const conditional = plan.trainingDayCalories !== plan.restDayCalories;
      const mismatch = value !== plan.trainingDayCalories || value !== plan.restDayCalories;
      return <span className={mismatch ? 'nutrition-admin__mismatch' : ''}>{conditional ? `${value} · ${copy.training} ${plan.trainingDayCalories} / ${copy.rest} ${plan.restDayCalories}` : `${value} / ${plan.mealBlockCalories}`}</span>;
    } },
    { title: copy.meals, dataIndex: 'mealBlockCount' },
    { title: copy.assignments, dataIndex: 'activeAssignmentCount' },
    { title: copy.status, dataIndex: 'contentStatus', render: (value: ContentStatus) => <Tag color={statusColor[value]}>{value}</Tag> },
    {
      title: copy.actions,
      render: (_, plan) => <Space wrap>
        <Button size="small" onClick={() => openEdit(plan.id)}>{copy.edit}</Button>
        <Button size="small" onClick={() => showValidation(plan.id)}>{copy.validate}</Button>
        {plan.contentStatus === 'Published' && <Button size="small" type="primary" onClick={() => setAssignmentPlan(plan)}>{copy.assign}</Button>}
        {nextStatus[plan.contentStatus] && <Popconfirm title={`${copy.next} ${nextStatus[plan.contentStatus]}?`} onConfirm={() => statusMutation.mutate({ id: plan.id, status: nextStatus[plan.contentStatus]!, expectedContentVersion: plan.contentVersion })}>
          <Button size="small">{nextStatus[plan.contentStatus]}</Button>
        </Popconfirm>}
      </Space>,
    },
  ], [ar, copy, statusMutation]);

  return <div className="nutrition-admin">
    <header className="nutrition-admin__header">
      <div><Title level={2}>{copy.title}</Title><Text type="secondary">{copy.subtitle}</Text></div>
      <Button type="primary" size="large" icon={<span className="material-symbols-outlined">add</span>} onClick={openNew}>{copy.newPlan}</Button>
    </header>

    <Card className="nutrition-admin__filters"><Space wrap>
      <Input.Search allowClear placeholder={copy.search} onSearch={(value) => { setSearch(value); setPage(1); }} />
      <Select allowClear placeholder={copy.allStatuses} value={status} onChange={(value) => { setStatus(value); setPage(1); }} options={Object.keys(statusColor).map(value => ({ value, label: value }))} />
    </Space></Card>

    <Card className="nutrition-admin__table">
      {isLoading ? <Skeleton active /> : <Table rowKey="id" columns={columns} dataSource={data?.items} pagination={false} locale={{ emptyText: <Empty description={copy.noPlans} /> }} scroll={{ x: 850 }} />}
      {!!data?.totalCount && <Pagination current={page} pageSize={10} total={data.totalCount} onChange={setPage} showSizeChanger={false} />}
    </Card>

    <Modal className="nutrition-admin__editor" width={1100} open={editorOpen} title={copy.details} onCancel={() => setEditorOpen(false)} footer={[
      <Button key="cancel" onClick={() => setEditorOpen(false)}>{copy.cancel}</Button>,
      <Button key="save" type="primary" loading={saveMutation.isPending} onClick={save}>{copy.save}</Button>,
    ]}>
      {editorLoading ? <Skeleton active /> : <Form form={form} layout="vertical" initialValues={initialPlan}>
        <Form.Item name="expectedContentVersion" hidden><Input /></Form.Item>
        <div className="nutrition-admin__grid">
          <Form.Item name="name" label={copy.nameEn} rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="nameAr" label={copy.nameAr} rules={[{ required: true }]}><Input dir="rtl" /></Form.Item>
          <Form.Item name="description" label={copy.descEn}><TextArea rows={2} /></Form.Item>
          <Form.Item name="descriptionAr" label={copy.descAr}><TextArea rows={2} dir="rtl" /></Form.Item>
          <Form.Item name="targetCalories" label={copy.targetCalories} rules={[{ required: true }]}><InputNumber min={1} /></Form.Item>
          <Form.Item name="minimumProteinGrams" label={copy.minProtein} rules={[{ required: true }]}><InputNumber min={1} /></Form.Item>
        </div>

        <Form.List name="mealBlocks">
          {(blocks, { add: addBlock, remove: removeBlock }) => <section>
            <div className="nutrition-admin__section-title"><Title level={4}>{copy.mealBlocks}</Title><Button onClick={() => addBlock(copyPlanForForm(initialPlan).mealBlocks[0])}>{copy.addMeal}</Button></div>
            {blocks.map((block, blockIndex) => <Card key={block.key} className="nutrition-admin__meal" title={`${copy.mealBlocks} ${blockIndex + 1}`} extra={blocks.length > 1 && <Button danger type="text" onClick={() => removeBlock(block.name)}>{copy.remove}</Button>}>
              <div className="nutrition-admin__grid nutrition-admin__grid--meal">
                <Form.Item name={[block.name, 'mealType']} label={copy.mealType}><Select options={mealTypes.map(value => ({ value, label: value }))} /></Form.Item>
                <Form.Item name={[block.name, 'targetCalories']} label={copy.mealCalories}><InputNumber min={0} /></Form.Item>
                <Form.Item name={[block.name, 'label']} label={copy.labelEn} rules={[{ required: true }]}><Input /></Form.Item>
                <Form.Item name={[block.name, 'labelAr']} label={copy.labelAr} rules={[{ required: true }]}><Input dir="rtl" /></Form.Item>
                <Form.Item name={[block.name, 'instructions']} label={copy.instructionsEn}><Input /></Form.Item>
                <Form.Item name={[block.name, 'instructionsAr']} label={copy.instructionsAr}><Input dir="rtl" /></Form.Item>
                <Form.Item name={[block.name, 'trainingDayOnly']} valuePropName="checked"><Checkbox>{copy.trainingOnly}</Checkbox></Form.Item>
                <Form.Item name={[block.name, 'restDayOnly']} valuePropName="checked"><Checkbox>{copy.restOnly}</Checkbox></Form.Item>
              </div>
              <Form.List name={[block.name, 'options']}>
                {(options, { add: addOption, remove: removeOption }) => <div className="nutrition-admin__options">
                  <div className="nutrition-admin__section-title"><strong>{copy.options}</strong><Button size="small" onClick={() => addOption(copyPlanForForm(initialPlan).mealBlocks[0].options[0])}>{copy.addOption}</Button></div>
                  {options.map((option, optionIndex) => <Card size="small" key={option.key} title={`${copy.options} ${optionIndex + 1}`} extra={options.length > 1 && <Button danger type="text" onClick={() => removeOption(option.name)}>{copy.remove}</Button>}>
                    <div className="nutrition-admin__grid">
                      <Form.Item name={[option.name, 'label']} label={copy.labelEn} rules={[{ required: true }]}><Input /></Form.Item>
                      <Form.Item name={[option.name, 'labelAr']} label={copy.labelAr} rules={[{ required: true }]}><Input dir="rtl" /></Form.Item>
                      <Form.Item name={[option.name, 'isCompleteOption']} valuePropName="checked"><Checkbox>{copy.completeOption}</Checkbox></Form.Item>
                    </div>
                    <Form.List name={[option.name, 'items']}>
                      {(items, { add: addItem, remove: removeItem }) => <div>
                        <Text type="secondary">{copy.sourceHint}</Text>
                        {items.map((item) => <div className="nutrition-admin__item" key={item.key}>
                          <Form.Item name={[item.name, 'foodId']} label={copy.food}><Select allowClear showSearch filterOption={false} onSearch={setFoodSearchInput} onChange={value => {
                            if (!value) return;
                            const path = ['mealBlocks', block.name, 'options', option.name, 'items', item.name];
                            setNestedField([...path, 'recipeId'], null); setNestedField([...path, 'itemName'], ''); setNestedField([...path, 'itemNameAr'], '');
                          }} onPopupScroll={event => loadMoreOptions(event, foodsQuery.hasNextPage, foodsQuery.isFetchingNextPage, foodsQuery.fetchNextPage)} loading={foodsQuery.isFetching} options={foodOptions} /></Form.Item>
                          <Form.Item name={[item.name, 'recipeId']} label={copy.recipe}><Select allowClear showSearch filterOption={false} onSearch={setRecipeSearchInput} onChange={value => {
                            if (!value) return;
                            const path = ['mealBlocks', block.name, 'options', option.name, 'items', item.name];
                            setNestedField([...path, 'foodId'], null); setNestedField([...path, 'itemName'], ''); setNestedField([...path, 'itemNameAr'], '');
                          }} onPopupScroll={event => loadMoreOptions(event, recipesQuery.hasNextPage, recipesQuery.isFetchingNextPage, recipesQuery.fetchNextPage)} loading={recipesQuery.isFetching} options={recipeOptions} /></Form.Item>
                          <Form.Item name={[item.name, 'itemName']} label={copy.customEn}><Input onChange={event => {
                            if (!event.target.value) return;
                            const path = ['mealBlocks', block.name, 'options', option.name, 'items', item.name];
                            setNestedField([...path, 'foodId'], null); setNestedField([...path, 'recipeId'], null);
                          }} /></Form.Item>
                          <Form.Item name={[item.name, 'itemNameAr']} label={copy.customAr}><Input dir="rtl" onChange={event => {
                            if (!event.target.value) return;
                            const path = ['mealBlocks', block.name, 'options', option.name, 'items', item.name];
                            setNestedField([...path, 'foodId'], null); setNestedField([...path, 'recipeId'], null);
                          }} /></Form.Item>
                          <Form.Item name={[item.name, 'quantity']} label={copy.quantity} rules={[{ required: true }]}><InputNumber min={0.01} /></Form.Item>
                          <Form.Item name={[item.name, 'unit']} label={copy.unit}><Select options={units.map(value => ({ value, label: value }))} /></Form.Item>
                          <Form.Item name={[item.name, 'measurementState']} label={copy.preparation}><Select options={preparationStates.map(value => ({ value, label: value }))} /></Form.Item>
                          <Form.Item name={[item.name, 'alternativeGroupKey']} label={copy.alternativeGroup}><Input /></Form.Item>
                          {items.length > 1 && <Button danger type="text" onClick={() => removeItem(item.name)}>{copy.remove}</Button>}
                        </div>)}
                        <Button size="small" onClick={() => addItem(copyPlanForForm(initialPlan).mealBlocks[0].options[0].items[0])}>{copy.addItem}</Button>
                      </div>}
                    </Form.List>
                  </Card>)}
                </div>}
              </Form.List>
            </Card>)}
          </section>}
        </Form.List>

        <Form.List name="rules">
          {(rules, { add, remove }) => <section>
            <div className="nutrition-admin__section-title"><Title level={4}>{copy.rules}</Title><Button onClick={() => add({ ruleType: 'general', text: '', textAr: '' })}>{copy.addRule}</Button></div>
            {rules.map(rule => <div className="nutrition-admin__rule" key={rule.key}>
              <Form.Item name={[rule.name, 'ruleType']} label={copy.ruleType}><Input /></Form.Item>
              <Form.Item name={[rule.name, 'text']} label={copy.ruleEn}><Input /></Form.Item>
              <Form.Item name={[rule.name, 'textAr']} label={copy.ruleAr} rules={[{ required: true }]}><Input dir="rtl" /></Form.Item>
              <Button danger type="text" onClick={() => remove(rule.name)}>{copy.remove}</Button>
            </div>)}
          </section>}
        </Form.List>
      </Form>}
    </Modal>

    <Modal open={validationOpen} title={copy.validationTitle} footer={null} onCancel={() => setValidationOpen(false)}>
      {validation && <>
        <Alert type={validation.isValidForPublish ? 'success' : 'error'} showIcon message={validation.isValidForPublish ? copy.valid : copy.invalid} description={`${copy.training}: ${validation.trainingDayCalories} kcal · ${copy.rest}: ${validation.restDayCalories} kcal · ${copy.targetCalories}: ${validation.targetCalories} kcal`} />
        <div className="nutrition-admin__issues">{validation.issues.map((issue, index) => <Alert key={`${issue.code}-${index}`} type="error" showIcon message={issue.message} description={issue.path} />)}</div>
      </>}
    </Modal>

    <Modal open={!!assignmentPlan} title={copy.assignTitle} confirmLoading={assignMutation.isPending} okText={copy.confirmAssign} onCancel={() => setAssignmentPlan(null)} onOk={async () => {
      if (!assignmentPlan || !athleteIds.length) return;
      await assignMutation.mutateAsync({ id: assignmentPlan.id, athleteIds, notes: assignmentNotes || undefined });
      setAssignmentPlan(null); setAthleteIds([]); setAssignmentNotes('');
    }}>
      <Form layout="vertical">
        <Form.Item label={copy.selectAthletes} required><Select mode="multiple" value={athleteIds} onChange={setAthleteIds} options={roster?.items.map(item => ({ value: item.athleteId, label: item.athleteName }))} /></Form.Item>
        <Form.Item label={copy.notes}><TextArea value={assignmentNotes} onChange={event => setAssignmentNotes(event.target.value)} rows={3} /></Form.Item>
      </Form>
    </Modal>
  </div>;
};

export default NutritionPlanAdmin;
