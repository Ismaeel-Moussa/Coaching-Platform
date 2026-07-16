import React, { useEffect, useMemo, useState } from 'react';
import {
  Button,
  Card,
  Checkbox,
  Collapse,
  Form,
  Input,
  InputNumber,
  Select,
  Skeleton,
  Space,
  Tabs,
  Typography,
  Radio,
} from 'antd';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import { useInfiniteQuery } from '@tanstack/react-query';
import { searchFoods } from '../../../api/food';
import { getRecipes } from '../../../api/recipe';
import {
  useNutritionPlan,
  useSaveNutritionPlan,
} from '../../../hooks/useNutritionPlans/useNutritionPlans';
import type {
  ContentStatus,
  NutritionPlanForm,
  MealType,
  IngredientUnit,
  FoodPreparationState,
} from '../../../types/NutritionPlan';
import './NutritionPlanEditor.scss';

const { TextArea } = Input;
const { Title, Text } = Typography;

const initialPlan: NutritionPlanForm = {
  name: '',
  description: '',
  targetCalories: 2000,
  minimumProteinGrams: 150,
  mealBlocks: [{
    mealType: 'Breakfast',
    label: 'Meal 1',
    targetCalories: 2000,
    trainingDayOnly: false,
    restDayOnly: false,
    instructions: '',
    options: [{
      label: 'Option 1',
      isCompleteOption: true,
      items: [{
        foodId: null,
        recipeId: null,
        itemName: '',
        quantity: 1,
        unit: 'Piece',
        measurementState: 'Unspecified',
      }],
    }],
  }],
  rules: [],
};

const copyPlanForForm = (plan: NutritionPlanForm): NutritionPlanForm => JSON.parse(JSON.stringify(plan));

const mealTypes: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack', 'Suhoor', 'Iftar', 'PreWorkout', 'PostWorkout'];
const units: IngredientUnit[] = ['Gram', 'Milliliter', 'Piece', 'Tablespoon', 'Teaspoon', 'Cup', 'Scoop'];
const preparationStates: FoodPreparationState[] = ['Unspecified', 'Raw', 'Cooked', 'Drained'];

interface OptionItemRowProps {
  blockIndex: number;
  optionIndex: number;
  itemIndex: number;
  onRemove: () => void;
  showRemove: boolean;
  form: any;
  foodOptions: { value: number; label: string }[];
  recipeOptions: { value: number; label: string }[];
  onSearchFood: (search: string) => void;
  onSearchRecipe: (search: string) => void;
  foodsLoading: boolean;
  recipesLoading: boolean;
  loadMoreFoods: (e: React.UIEvent<HTMLDivElement>) => void;
  loadMoreRecipes: (e: React.UIEvent<HTMLDivElement>) => void;
  copy: any;
}

const OptionItemRow: React.FC<OptionItemRowProps> = ({
  blockIndex,
  optionIndex,
  itemIndex,
  onRemove,
  showRemove,
  form,
  foodOptions,
  recipeOptions,
  onSearchFood,
  onSearchRecipe,
  foodsLoading,
  recipesLoading,
  loadMoreFoods,
  loadMoreRecipes,
  copy,
}) => {
  const itemPath = ['mealBlocks', blockIndex, 'options', optionIndex, 'items', itemIndex];

  // Derive initial source type
  const [sourceType, setSourceType] = useState<'food' | 'recipe' | 'custom'>(() => {
    const fId = form.getFieldValue([...itemPath, 'foodId']);
    const rId = form.getFieldValue([...itemPath, 'recipeId']);
    if (fId) return 'food';
    if (rId) return 'recipe';
    return 'custom';
  });

  const handleSourceTypeChange = (type: 'food' | 'recipe' | 'custom') => {
    setSourceType(type);
    if (type === 'food') {
      form.setFieldValue([...itemPath, 'recipeId'], null);
      form.setFieldValue([...itemPath, 'itemName'], null);
    } else if (type === 'recipe') {
      form.setFieldValue([...itemPath, 'foodId'], null);
      form.setFieldValue([...itemPath, 'itemName'], null);
    } else {
      form.setFieldValue([...itemPath, 'foodId'], null);
      form.setFieldValue([...itemPath, 'recipeId'], null);
    }
  };

  return (
    <div className="nutrition-editor-item-row">
      <div className="nutrition-editor-item-row__source-selector">
        <Radio.Group
          value={sourceType}
          onChange={(e) => handleSourceTypeChange(e.target.value)}
          className="nutrition-editor-item-row__radio-group"
        >
          <Radio.Button value="food">{copy.food}</Radio.Button>
          <Radio.Button value="recipe">{copy.recipe}</Radio.Button>
          <Radio.Button value="custom">{copy.customText}</Radio.Button>
        </Radio.Group>

        <div className="nutrition-editor-item-row__input-container">
          {sourceType === 'food' && (
            <Form.Item name={[itemIndex, 'foodId']} noStyle>
              <Select
                showSearch
                allowClear
                placeholder={copy.selectFood}
                filterOption={false}
                onSearch={onSearchFood}
                onPopupScroll={loadMoreFoods}
                loading={foodsLoading}
                options={foodOptions}
                className="nutrition-editor-item-row__select"
              />
            </Form.Item>
          )}

          {sourceType === 'recipe' && (
            <Form.Item name={[itemIndex, 'recipeId']} noStyle>
              <Select
                showSearch
                allowClear
                placeholder={copy.selectRecipe}
                filterOption={false}
                onSearch={onSearchRecipe}
                onPopupScroll={loadMoreRecipes}
                loading={recipesLoading}
                options={recipeOptions}
                className="nutrition-editor-item-row__select"
              />
            </Form.Item>
          )}

          {sourceType === 'custom' && (
            <Form.Item name={[itemIndex, 'itemName']} noStyle rules={[{ required: true, message: copy.required }]}>
              <Input placeholder={copy.customPlaceholder} />
            </Form.Item>
          )}
        </div>
      </div>

      <div className="nutrition-editor-item-row__grid">
        <Form.Item label={copy.quantity} name={[itemIndex, 'quantity']} rules={[{ required: true }]} className="nutrition-editor-item-row__field">
          <InputNumber min={0.01} style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item label={copy.unit} name={[itemIndex, 'unit']} className="nutrition-editor-item-row__field">
          <Select options={units.map(value => ({ value, label: value }))} />
        </Form.Item>

        <Form.Item label={copy.preparation} name={[itemIndex, 'measurementState']} className="nutrition-editor-item-row__field">
          <Select options={preparationStates.map(value => ({ value, label: value }))} />
        </Form.Item>

        <Form.Item label={copy.alternativeGroup} name={[itemIndex, 'alternativeGroupKey']} className="nutrition-editor-item-row__field">
          <Input placeholder="e.g. Group A" />
        </Form.Item>

        {showRemove && (
          <Button danger type="text" onClick={onRemove} className="nutrition-editor-item-row__remove-btn">
            {copy.remove}
          </Button>
        )}
      </div>
    </div>
  );
};

const NutritionPlanEditor: React.FC = () => {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();
  const { i18n } = useTranslation();
  const ar = i18n.resolvedLanguage === 'ar';
  const [form] = Form.useForm<NutritionPlanForm>();
  const editingId = id ? parseInt(id, 10) : undefined;

  const [foodSearchInput, setFoodSearchInput] = useState('');
  const [recipeSearchInput, setRecipeSearchInput] = useState('');
  const [foodSearch, setFoodSearch] = useState('');
  const [recipeSearch, setRecipeSearch] = useState('');

  const copy = ar ? {
    titleCreate: 'خطة تغذية جديدة', titleEdit: 'تعديل خطة التغذية',
    details: 'تفاصيل الخطة', save: 'حفظ الخطة', cancel: 'إلغاء',
    name: 'اسم الخطة', desc: 'وصف الخطة', targetCalories: 'السعرات المستهدفة', minProtein: 'الحد الأدنى للبروتين (غ)',
    mealBlocks: 'الوجبات', addMeal: 'إضافة وجبة جديدة', mealType: 'نوع الوجبة', label: 'العنوان',
    mealCalories: 'سعرات الوجبة', trainingOnly: 'يوم التدريب فقط', restOnly: 'يوم الراحة فقط',
    instructions: 'التعليمات والإرشادات', options: 'الخيارات البديلة', addOption: 'إضافة خيار بديل',
    completeOption: 'خيار متكامل', items: 'المكونات والأطعمة', addItem: 'إضافة صنف',
    food: 'طعام من الدليل', recipe: 'وصفة صحية', customText: 'عنصر يدوي مخصص', selectFood: 'اختر طعاماً...',
    selectRecipe: 'اختر وصفة...', customPlaceholder: 'اكتب الصنف والاسم يدوياً...', required: 'حقل مطلوب',
    quantity: 'الكمية', unit: 'الوحدة', preparation: 'التحضير', alternativeGroup: 'مجموعة البدائل',
    rules: 'القواعد العامة للخطة', addRule: 'إضافة قاعدة', ruleType: 'نوع القاعدة', ruleText: 'نص القاعدة',
    remove: 'حذف', generalTab: 'المعلومات العامة', mealsTab: 'جدول الوجبات', rulesTab: 'إرشادات عامة',
    sourceHint: 'اختر مصدراً للطعام أو اكتب اسماً مخصصاً.',
  } : {
    titleCreate: 'Create Nutrition Plan', titleEdit: 'Edit Nutrition Plan',
    details: 'Plan details', save: 'Save Plan', cancel: 'Cancel',
    name: 'Plan Name', desc: 'Plan Description', targetCalories: 'Target Calories (kcal)', minProtein: 'Minimum Protein (g)',
    mealBlocks: 'Meal blocks', addMeal: 'Add meal block', mealType: 'Meal Type', label: 'Label',
    mealCalories: 'Meal Calories', trainingOnly: 'Training day only', restOnly: 'Rest day only',
    instructions: 'Instructions & Notes', options: 'Options', addOption: 'Add option',
    completeOption: 'Complete option', items: 'Items', addItem: 'Add item',
    food: 'Food Catalog', recipe: 'Recipe Library', customText: 'Custom Text', selectFood: 'Select food...',
    selectRecipe: 'Select recipe...', customPlaceholder: 'Enter custom item name...', required: 'Required',
    quantity: 'Quantity', unit: 'Unit', preparation: 'Preparation', alternativeGroup: 'Alternative Group',
    rules: 'Rules', addRule: 'Add rule', ruleType: 'Rule Type', ruleText: 'Rule Text',
    remove: 'Remove', generalTab: 'General Info', mealsTab: 'Meals & Options', rulesTab: 'Plan Rules',
    sourceHint: 'Select food, recipe, or write a custom item name.',
  };

  const { data: editedPlan, isLoading: editorLoading } = useNutritionPlan(editingId);
  const saveMutation = useSaveNutritionPlan();

  const foodsQuery = useInfiniteQuery({
    queryKey: ['nutrition-editor-food-options', foodSearch],
    queryFn: ({ pageParam }) => searchFoods({ search: foodSearch || undefined, page: pageParam, pageSize: 50 }),
    initialPageParam: 1,
    getNextPageParam: lastPage => lastPage.hasNextPage ? lastPage.page + 1 : undefined,
  });

  const recipesQuery = useInfiniteQuery({
    queryKey: ['nutrition-editor-recipe-options', recipeSearch],
    queryFn: ({ pageParam }) => getRecipes({ isJokerRecipe: true, search: recipeSearch || undefined, page: pageParam, pageSize: 50 }),
    initialPageParam: 1,
    getNextPageParam: lastPage => lastPage.hasNextPage ? lastPage.page + 1 : undefined,
  });

  useEffect(() => {
    if (editedPlan) {
      form.setFieldsValue({
        ...copyPlanForForm(editedPlan),
        expectedContentVersion: editedPlan.contentVersion,
      });
    } else {
      form.setFieldsValue(copyPlanForForm(initialPlan));
    }
  }, [editedPlan, form]);

  useEffect(() => {
    const timer = window.setTimeout(() => setFoodSearch(foodSearchInput.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [foodSearchInput]);

  useEffect(() => {
    const timer = window.setTimeout(() => setRecipeSearch(recipeSearchInput.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [recipeSearchInput]);

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
    if (hasNextPage && !isFetchingNextPage && target.scrollTop + target.offsetHeight >= target.scrollHeight - 24) {
      fetchNextPage();
    }
  };

  const handleSave = async () => {
    const values = await form.validateFields();
    await saveMutation.mutateAsync({ id: editingId, form: values });
    navigate('/coach/nutrition-plans');
  };

  if (editorLoading) {
    return (
      <div className="nutrition-editor">
        <Skeleton active />
      </div>
    );
  }

  return (
    <div className="nutrition-editor">
      <header className="nutrition-editor__header">
        <div>
          <Title level={2}>{editingId ? copy.titleEdit : copy.titleCreate}</Title>
          <Text type="secondary">{copy.details}</Text>
        </div>
        <Space>
          <Button onClick={() => navigate('/coach/nutrition-plans')}>{copy.cancel}</Button>
          <Button type="primary" loading={saveMutation.isPending} onClick={handleSave}>
            {copy.save}
          </Button>
        </Space>
      </header>

      <Form form={form} layout="vertical" initialValues={initialPlan}>
        <Form.Item name="expectedContentVersion" hidden><Input /></Form.Item>

        <Tabs defaultActiveKey="general" type="card" className="nutrition-editor__tabs">
          <Tabs.TabPane tab={copy.generalTab} key="general">
            <Card className="nutrition-editor__card">
              <div className="nutrition-editor__general-grid">
                <Form.Item name="name" label={copy.name} rules={[{ required: true }]} className="nutrition-editor__full-width">
                  <Input size="large" />
                </Form.Item>

                <Form.Item name="description" label={copy.desc} className="nutrition-editor__full-width">
                  <TextArea rows={3} />
                </Form.Item>

                <Form.Item name="targetCalories" label={copy.targetCalories} rules={[{ required: true }]}>
                  <InputNumber min={1} style={{ width: '100%' }} size="large" />
                </Form.Item>

                <Form.Item name="minimumProteinGrams" label={copy.minProtein} rules={[{ required: true }]}>
                  <InputNumber min={1} style={{ width: '100%' }} size="large" />
                </Form.Item>
              </div>
            </Card>
          </Tabs.TabPane>

          <Tabs.TabPane tab={copy.mealsTab} key="meals">
            <Form.List name="mealBlocks">
              {(blocks, { add: addBlock, remove: removeBlock }) => (
                <div className="nutrition-editor__meals-list">
                  <Collapse defaultActiveKey={['0']} className="nutrition-editor__collapse" expandIconPosition="end">
                    {blocks.map((block, blockIndex) => (
                      <Collapse.Panel
                        header={
                          <div className="nutrition-editor__panel-header">
                            <Text strong>{`${copy.mealBlocks} ${blockIndex + 1}: `}</Text>
                            <Form.Item name={[block.name, 'label']} noStyle>
                              <Input style={{ width: 200, display: 'inline-block', margin: '0 8px' }} onClick={(e) => e.stopPropagation()} />
                            </Form.Item>
                            {blocks.length > 1 && (
                              <Button
                                danger
                                type="text"
                                size="small"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  removeBlock(block.name);
                                }}
                              >
                                {copy.remove}
                              </Button>
                            )}
                          </div>
                        }
                        key={block.key.toString()}
                      >
                        <div className="nutrition-editor__meal-fields-grid">
                          <Form.Item name={[block.name, 'mealType']} label={copy.mealType}>
                            <Select options={mealTypes.map(value => ({ value, label: value }))} />
                          </Form.Item>
                          <Form.Item name={[block.name, 'targetCalories']} label={copy.mealCalories}>
                            <InputNumber min={0} style={{ width: '100%' }} />
                          </Form.Item>
                          <Form.Item name={[block.name, 'instructions']} label={copy.instructions} className="nutrition-editor__span-2">
                            <Input />
                          </Form.Item>
                          <div className="nutrition-editor__span-2 nutrition-editor__checkboxes">
                            <Form.Item name={[block.name, 'trainingDayOnly']} valuePropName="checked" noStyle>
                              <Checkbox>{copy.trainingOnly}</Checkbox>
                            </Form.Item>
                            <Form.Item name={[block.name, 'restDayOnly']} valuePropName="checked" noStyle>
                              <Checkbox>{copy.restOnly}</Checkbox>
                            </Form.Item>
                          </div>
                        </div>

                        {/* Options List */}
                        <Form.List name={[block.name, 'options']}>
                          {(options, { add: addOption, remove: removeOption }) => (
                            <div className="nutrition-editor__options-section">
                              <div className="nutrition-editor__options-header">
                                <Text strong>{copy.options}</Text>
                                <Button size="small" onClick={() => addOption(copyPlanForForm(initialPlan).mealBlocks[0].options[0])}>
                                  {copy.addOption}
                                </Button>
                              </div>

                              {options.map((option, optionIndex) => (
                                <Card
                                  size="small"
                                  key={option.key}
                                  className="nutrition-editor__option-card"
                                  title={`${copy.options} ${optionIndex + 1}`}
                                  extra={
                                    options.length > 1 && (
                                      <Button danger type="text" size="small" onClick={() => removeOption(option.name)}>
                                        {copy.remove}
                                      </Button>
                                    )
                                  }
                                >
                                  <div className="nutrition-editor__option-header-fields">
                                    <Form.Item name={[option.name, 'label']} label={copy.label} rules={[{ required: true }]} style={{ marginBottom: 12 }}>
                                      <Input style={{ maxWidth: 300 }} />
                                    </Form.Item>
                                    <Form.Item name={[option.name, 'isCompleteOption']} valuePropName="checked" style={{ marginBottom: 12 }}>
                                      <Checkbox>{copy.completeOption}</Checkbox>
                                    </Form.Item>
                                  </div>

                                  {/* Items List */}
                                  <Form.List name={[option.name, 'items']}>
                                    {(items, { add: addItem, remove: removeItem }) => (
                                      <div className="nutrition-editor__items-section">
                                        <Text type="secondary" className="nutrition-editor__source-hint">{copy.sourceHint}</Text>
                                        {items.map((item, itemIndex) => (
                                          <OptionItemRow
                                            key={item.key}
                                            blockIndex={blockIndex}
                                            optionIndex={optionIndex}
                                            itemIndex={item.name}
                                            onRemove={() => removeItem(item.name)}
                                            showRemove={items.length > 1}
                                            form={form}
                                            foodOptions={foodOptions}
                                            recipeOptions={recipeOptions}
                                            onSearchFood={setFoodSearchInput}
                                            onSearchRecipe={setRecipeSearchInput}
                                            foodsLoading={foodsQuery.isFetching}
                                            recipesLoading={recipesQuery.isFetching}
                                            loadMoreFoods={(e) => loadMoreOptions(e, foodsQuery.hasNextPage, foodsQuery.isFetchingNextPage, foodsQuery.fetchNextPage)}
                                            loadMoreRecipes={(e) => loadMoreOptions(e, recipesQuery.hasNextPage, recipesQuery.isFetchingNextPage, recipesQuery.fetchNextPage)}
                                            copy={copy}
                                          />
                                        ))}
                                        <Button
                                          size="small"
                                          type="dashed"
                                          onClick={() => addItem(copyPlanForForm(initialPlan).mealBlocks[0].options[0].items[0])}
                                          style={{ marginTop: 8 }}
                                        >
                                          {copy.addItem}
                                        </Button>
                                      </div>
                                    )}
                                  </Form.List>
                                </Card>
                              ))}
                            </div>
                          )}
                        </Form.List>
                      </Collapse.Panel>
                    ))}
                  </Collapse>
                  <Button type="primary" onClick={() => addBlock(copyPlanForForm(initialPlan).mealBlocks[0])}>
                    {copy.addMeal}
                  </Button>
                </div>
              )}
            </Form.List>
          </Tabs.TabPane>

          <Tabs.TabPane tab={copy.rulesTab} key="rules">
            <Card className="nutrition-editor__card">
              <Form.List name="rules">
                {(rules, { add: addRule, remove: removeRule }) => (
                  <div className="nutrition-editor__rules-section">
                    <div className="nutrition-editor__rules-header">
                      <Title level={4} style={{ margin: 0 }}>{copy.rules}</Title>
                      <Button onClick={() => addRule({ ruleType: 'general', text: '' })}>{copy.addRule}</Button>
                    </div>

                    {rules.map((rule) => (
                      <div className="nutrition-editor__rule-row" key={rule.key}>
                        <Form.Item name={[rule.name, 'ruleType']} label={copy.ruleType} style={{ flex: 1, marginBottom: 0 }}>
                          <Input />
                        </Form.Item>
                        <Form.Item name={[rule.name, 'text']} label={copy.ruleText} rules={[{ required: true }]} style={{ flex: 3, marginBottom: 0 }}>
                          <Input />
                        </Form.Item>
                        <Button danger type="text" onClick={() => removeRule(rule.name)} style={{ alignSelf: 'flex-end' }}>
                          {copy.remove}
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </Form.List>
            </Card>
          </Tabs.TabPane>
        </Tabs>
      </Form>
    </div>
  );
};

export default NutritionPlanEditor;
