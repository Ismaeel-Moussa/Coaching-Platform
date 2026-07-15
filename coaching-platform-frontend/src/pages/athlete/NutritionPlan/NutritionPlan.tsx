import React, { useState } from 'react';
import { Alert, Button, Card, DatePicker, Empty, InputNumber, Modal, Popconfirm, Radio, Segmented, Select, Skeleton, Tag, Typography } from 'antd';
import { useTranslation } from 'react-i18next';
import dayjs, { type Dayjs } from 'dayjs';
import { useMyNutritionPlan } from '../../../hooks/useNutritionPlans/useNutritionPlans';
import { useLogNutritionPlanOption, useNutritionPlanEntries, useRemoveNutritionPlanEntry } from '../../../hooks/useDiary/useDiary';
import { MealType as DiaryMealType } from '../../../types/Diary';
import type { NutritionMealBlock, NutritionMealOption, NutritionOptionItem } from '../../../types/NutritionPlan';
import './NutritionPlan.scss';

const { Title, Text, Paragraph } = Typography;

const NutritionPlan: React.FC = () => {
  const { i18n, t } = useTranslation(['athlete', 'common']);
  const ar = i18n.resolvedLanguage === 'ar';
  const { data: assignment, isLoading, isError } = useMyNutritionPlan();
  const [logDate, setLogDate] = useState<Dayjs>(dayjs());
  const dateKey = logDate.format('YYYY-MM-DD');
  const { data: completedEntries = [] } = useNutritionPlanEntries(assignment?.id, dateKey);
  const logOption = useLogNutritionPlanOption(assignment?.id, dateKey);
  const removePlanMeal = useRemoveNutritionPlanEntry(dateKey);
  const [selected, setSelected] = useState<{ block: NutritionMealBlock; option: NutritionMealOption } | null>(null);
  const [mealType, setMealType] = useState<DiaryMealType>(DiaryMealType.Breakfast);
  const [servings, setServings] = useState(1);
  const [selectedAlternatives, setSelectedAlternatives] = useState<Record<string, number>>({});
  const [dayType, setDayType] = useState<'training' | 'rest'>('training');
  const copy = ar ? {
    title: 'خطتي الغذائية', subtitle: 'الخطة الغذائية التي عيّنها لك المدرب', noPlan: 'لم يتم تعيين خطة غذائية لك حتى الآن.',
    calories: 'سعرة حرارية', protein: 'غ بروتين على الأقل', assigned: 'تاريخ التعيين', coachNotes: 'ملاحظات المدرب',
    choose: 'اختر خياراً واحداً', quantity: 'الكمية', rules: 'قواعد الخطة', error: 'تعذر تحميل خطتك الغذائية. حاول مرة أخرى.',
    training: 'يوم التدريب', rest: 'يوم الراحة', option: 'خيار',
  } : {
    title: 'My nutrition plan', subtitle: 'The nutrition plan assigned by your coach', noPlan: 'No nutrition plan has been assigned to you yet.',
    calories: 'kcal', protein: 'g minimum protein', assigned: 'Assigned', coachNotes: 'Coach notes', choose: 'Choose one option',
    quantity: 'Quantity', rules: 'Plan rules', error: 'We could not load your nutrition plan. Please try again.', training: 'Training day',
    rest: 'Rest day', option: 'Option',
  };

  if (isLoading) return <div className="athlete-plan"><Skeleton active paragraph={{ rows: 10 }} /></div>;
  if (isError) return <div className="athlete-plan"><Alert type="error" showIcon message={copy.error} /></div>;
  if (!assignment) return <div className="athlete-plan athlete-plan--empty"><Empty description={copy.noPlan} /></div>;

  const { plan } = assignment;
  const itemName = (item: NutritionOptionItem) => {
    if (item.foodName) return ar ? item.foodNameAr || item.foodName : item.foodName;
    if (item.recipeName) return ar ? item.recipeNameAr || item.recipeName : item.recipeName;
    return ar ? item.itemNameAr || item.itemName : item.itemName || item.itemNameAr;
  };
  const unitLabel = (unit: NutritionOptionItem['unit']) => {
    if (!ar) return unit;
    return ({
      Gram: 'غ', Milliliter: 'مل', Piece: 'قطعة', Tablespoon: 'ملعقة كبيرة',
      Teaspoon: 'ملعقة صغيرة', Cup: 'كوب', Scoop: 'مكيال',
    } as Record<NutritionOptionItem['unit'], string>)[unit];
  };
  const itemIsLoggable = (item: NutritionOptionItem) =>
    (item.foodId != null && item.unit === 'Gram') ||
    (item.recipeId != null && (item.unit === 'Gram' || item.unit === 'Piece'));
  const normalizedGroupKey = (item: NutritionOptionItem) =>
    item.alternativeGroupKey?.trim().toLocaleLowerCase() || null;
  const optionIsLoggable = (option: NutritionMealOption) => {
    const fixedItems = option.items.filter(item => !normalizedGroupKey(item));
    if (fixedItems.some(item => !itemIsLoggable(item))) return false;
    const groups = new Map<string, NutritionOptionItem[]>();
    option.items.filter(item => normalizedGroupKey(item)).forEach(item => {
      const key = normalizedGroupKey(item)!;
      groups.set(key, [...(groups.get(key) ?? []), item]);
    });
    return option.items.length > 0 && [...groups.values()].every(items => items.some(itemIsLoggable));
  };
  const diaryMealType = (type: NutritionMealBlock['mealType']) => ({
    Breakfast: DiaryMealType.Breakfast,
    Lunch: DiaryMealType.Lunch,
    Dinner: DiaryMealType.Dinner,
    Snack: DiaryMealType.Snack,
    Suhoor: DiaryMealType.Snack,
    Iftar: DiaryMealType.Snack,
    PreWorkout: DiaryMealType.Snack,
    PostWorkout: DiaryMealType.Snack,
  })[type];
  const mealChoices = [
    [DiaryMealType.Breakfast, 'common:meals.breakfast'],
    [DiaryMealType.Lunch, 'common:meals.lunch'],
    [DiaryMealType.Dinner, 'common:meals.dinner'],
    [DiaryMealType.Snack, 'common:meals.snack'],
  ].map(([value, key]) => ({ value: value as DiaryMealType, label: t(key as string) }));
  const alternativeGroups = (() => {
    const groups = new Map<string, { label: string; items: NutritionOptionItem[] }>();
    selected?.option.items.forEach((item) => {
      const key = normalizedGroupKey(item);
      if (!key) return;
      const current = groups.get(key);
      groups.set(key, { label: current?.label ?? item.alternativeGroupKey!, items: [...(current?.items ?? []), item] });
    });
    return [...groups.entries()];
  })();
  const openAddDialog = (block: NutritionMealBlock, option: NutritionMealOption) => {
    const alternatives: Record<string, number> = {};
    option.items.forEach((item) => {
      const group = normalizedGroupKey(item);
      if (group && item.id != null && itemIsLoggable(item) && alternatives[group] == null)
        alternatives[group] = item.id;
    });
    setSelected({ block, option });
    setMealType(diaryMealType(block.mealType));
    setServings(1);
    setSelectedAlternatives(alternatives);
  };
  const closeAddDialog = () => {
    if (!logOption.isPending) setSelected(null);
  };
  const confirmAdd = () => {
    if (!assignment || !selected) return;
    logOption.mutate({
      assignmentId: assignment.id,
      mealBlockId: selected.block.id!,
      mealOptionId: selected.option.id!,
      date: dateKey,
      mealType,
      servings,
      selectedAlternativeItemIds: Object.values(selectedAlternatives),
    }, { onSuccess: () => setSelected(null) });
  };
  const completedByBlock = new Map(completedEntries.map(entry => [entry.mealBlockId, entry]));
  const hasConditionalBlocks = plan.mealBlocks.some(block => block.trainingDayOnly || block.restDayOnly);
  const loggedConditionalBlock = plan.mealBlocks.find(block =>
    (block.trainingDayOnly || block.restDayOnly) && block.id != null && completedByBlock.has(block.id));
  const lockedDayType = loggedConditionalBlock
    ? (loggedConditionalBlock.trainingDayOnly ? 'training' : 'rest')
    : null;
  const effectiveDayType = lockedDayType ?? dayType;
  const visibleBlocks = plan.mealBlocks.filter(block =>
    (!block.trainingDayOnly && !block.restDayOnly) ||
    (effectiveDayType === 'training' && block.trainingDayOnly) ||
    (effectiveDayType === 'rest' && block.restDayOnly));
  const disabledDate = (date: Dayjs) => {
    const start = dayjs(assignment.startDate).startOf('day');
    const end = assignment.endDate ? dayjs(assignment.endDate).endOf('day') : null;
    return date.isBefore(start) || !!end?.isBefore(date);
  };
  const completedBlockCount = visibleBlocks.filter(block => block.id != null && completedByBlock.has(block.id)).length;
  const completionPercent = visibleBlocks.length > 0
    ? Math.round((completedBlockCount / visibleBlocks.length) * 100)
    : 0;

  return <div className="athlete-plan">
    <header className="athlete-plan__hero">
      <div className="athlete-plan__hero-content">
        <Text className="athlete-plan__eyebrow">{copy.subtitle}</Text>
        <Title>{ar ? plan.nameAr || plan.name : plan.name}</Title>
        {(ar ? plan.descriptionAr || plan.description : plan.description) && <div className="athlete-plan__description">
          <span className="material-symbols-outlined">format_quote</span>
          <Paragraph>{ar ? plan.descriptionAr || plan.description : plan.description}</Paragraph>
        </div>}
      </div>
      <div className="athlete-plan__targets">
        <div><span className="material-symbols-outlined">local_fire_department</span><strong>{plan.targetCalories}</strong><small>{copy.calories}</small></div>
        <div><span className="material-symbols-outlined">fitness_center</span><strong>{plan.minimumProteinGrams}</strong><small>{copy.protein}</small></div>
      </div>
    </header>

    <section className="athlete-plan__toolbar">
      <div className="athlete-plan__meta">
        <span className="material-symbols-outlined">event_available</span>
        <div><small>{copy.assigned}</small><strong>{new Date(assignment.assignedAt).toLocaleDateString(ar ? 'ar-EG' : 'en-US')}</strong></div>
      </div>
      <div className="athlete-plan__controls">
        <label><span>{t('athlete:nutritionPlan.logDate')}</span><DatePicker value={logDate} onChange={date => date && setLogDate(date)} disabledDate={disabledDate} allowClear={false} /></label>
        {hasConditionalBlocks && <label><span>{t('athlete:nutritionPlan.dayType')}</span><Segmented disabled={!!lockedDayType} value={effectiveDayType} onChange={value => setDayType(value as 'training' | 'rest')} options={[{ value: 'training', label: copy.training }, { value: 'rest', label: copy.rest }]} /></label>}
      </div>
      <div className="athlete-plan__progress">
        <div><span>{t('athlete:nutritionPlan.dailyProgress')}</span><strong>{completedBlockCount}/{visibleBlocks.length}</strong></div>
        <div className="athlete-plan__progress-track"><span style={{ width: `${completionPercent}%` }} /></div>
      </div>
    </section>
    {assignment.notes && <Alert className="athlete-plan__notes" type="info" showIcon message={copy.coachNotes} description={assignment.notes} />}

    <main className="athlete-plan__meals">
      {visibleBlocks.map((block, blockIndex) => <Card key={block.id ?? block.orderIndex} className="athlete-plan__meal">
        <div className="athlete-plan__meal-header">
          <div className="athlete-plan__meal-title">
            <span className="athlete-plan__meal-number">{String(blockIndex + 1).padStart(2, '0')}</span>
            <div><Text>{t('athlete:nutritionPlan.mealBlock')}</Text><Title level={3}>{ar ? block.labelAr || block.label : block.label}</Title></div>
          </div>
          <div className="athlete-plan__meal-tags">
            {block.targetCalories != null && <Tag color="gold" icon={<span className="material-symbols-outlined">bolt</span>}>{block.targetCalories} {copy.calories}</Tag>}
            {block.trainingDayOnly && <Tag color="blue">{copy.training}</Tag>}
            {block.restDayOnly && <Tag color="purple">{copy.rest}</Tag>}
          </div>
        </div>
        {(ar ? block.instructionsAr || block.instructions : block.instructions) && <Paragraph className="athlete-plan__instructions">{ar ? block.instructionsAr || block.instructions : block.instructions}</Paragraph>}
        <div className="athlete-plan__choose-label"><span className="material-symbols-outlined">touch_app</span><Text>{copy.choose}</Text></div>
        <div className="athlete-plan__options">
          {block.options.map((option, index) => {
            const completed = block.id != null ? completedByBlock.get(block.id) : undefined;
            const isSelectedCompletion = completed?.mealOptionId === option.id;
            const canLog = optionIsLoggable(option);
            return <section key={option.id ?? option.orderIndex} className={`athlete-plan__option ${isSelectedCompletion ? 'athlete-plan__option--completed' : ''}`}>
            <h4><span className="athlete-plan__option-number">{index + 1}</span><span className="athlete-plan__option-title"><small>{copy.option} {index + 1}</small>{ar ? option.labelAr || option.label : option.label}</span></h4>
            <ul>{option.items.map((item) => <li key={item.id ?? item.orderIndex}>
              <span><i /><span>{itemName(item)}</span></span>
              <strong>{item.quantity} <small>{unitLabel(item.unit)}</small></strong>
            </li>)}</ul>
            <div className="athlete-plan__option-action">
              {isSelectedCompletion && <Tag color="success" icon={<span className="material-symbols-outlined">check_circle</span>}>{t('athlete:nutritionPlan.completed')}</Tag>}
              {!canLog && !completed && <Tag color="warning">{t('athlete:nutritionPlan.notDiaryReady')}</Tag>}
              {isSelectedCompletion ? <Popconfirm title={t('athlete:mealLogger.removePlanMealConfirm')} onConfirm={() => removePlanMeal.mutate(completed!.id)} okText={t('athlete:mealLogger.removePlanMeal')} okButtonProps={{ danger: true }}>
                <Button danger loading={removePlanMeal.isPending} icon={<span className="material-symbols-outlined">delete_sweep</span>}>{t('athlete:mealLogger.removePlanMeal')}</Button>
              </Popconfirm> : <Button type="primary" disabled={!!completed || !canLog} onClick={() => openAddDialog(block, option)} icon={<span className="material-symbols-outlined">add_circle</span>}>
                {completed ? t('athlete:nutritionPlan.completed') : t('athlete:nutritionPlan.addOption')}
              </Button>}
            </div>
          </section>})}
        </div>
      </Card>)}
    </main>

    {!!plan.rules.length && <Card className="athlete-plan__rules" title={copy.rules}>
      <ol>{plan.rules.map(rule => <li key={rule.id ?? rule.orderIndex}>{ar ? rule.textAr || rule.text : rule.text || rule.textAr}</li>)}</ol>
    </Card>}

    <Modal
      open={!!selected}
      onCancel={closeAddDialog}
      title={t('athlete:nutritionPlan.addTitle')}
      okText={logOption.isPending ? t('athlete:nutritionPlan.adding') : t('athlete:nutritionPlan.confirmAdd')}
      cancelText={t('athlete:nutritionPlan.cancel')}
      onOk={confirmAdd}
      confirmLoading={logOption.isPending}
      okButtonProps={{ disabled: alternativeGroups.some(([group]) => selectedAlternatives[group] == null) }}
      destroyOnHidden
    >
      {selected && <div className="athlete-plan-add">
        <Paragraph>{t('athlete:nutritionPlan.addDescription')}</Paragraph>
        <div className="athlete-plan-add__option-name">{ar ? selected.option.labelAr || selected.option.label : selected.option.label}</div>
        <label><span>{t('athlete:nutritionPlan.logDate')}</span><DatePicker value={logDate} onChange={date => date && setLogDate(date)} disabledDate={disabledDate} allowClear={false} /></label>
        <label><span>{t('athlete:nutritionPlan.meal')}</span><Select value={mealType} onChange={setMealType} options={mealChoices} /></label>
        <label><span>{t('athlete:nutritionPlan.servings')}</span><InputNumber min={0.25} max={10} step={0.25} value={servings} onChange={value => setServings(value ?? 1)} /></label>
        {alternativeGroups.map(([group, data]) => <fieldset key={group}>
          <legend>{t('athlete:nutritionPlan.alternatives', { group: data.label })}</legend>
          <Radio.Group value={selectedAlternatives[group]} onChange={event => setSelectedAlternatives(current => ({ ...current, [group]: event.target.value }))}>
            {data.items.map(item => <Radio disabled={!itemIsLoggable(item)} key={item.id} value={item.id}>{itemName(item)} · {item.quantity} {unitLabel(item.unit)}</Radio>)}
          </Radio.Group>
        </fieldset>)}
      </div>}
    </Modal>
  </div>;
};

export default NutritionPlan;
