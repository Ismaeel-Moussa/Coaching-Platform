import React, { useEffect, useMemo, useState } from 'react';
import { Button, Drawer, Empty, Input, InputNumber, Modal, Popover, Select, Spin } from 'antd';
import { useTranslation } from 'react-i18next';
import { useSearchFoods } from '../../hooks/useFoods/useFoods';
import { useGetRecipes } from '../../hooks/useRecipes/useRecipes';
import {
  useBulkLogFood,
  useGetFilteredNutritionItems,
  useToggleFavoriteFood,
  useToggleFavoriteRecipe,
} from '../../hooks/useDiary/useDiary';
import { calcMacroPreview } from '../../utils/macroCalc';
import { MealType, MEAL_TYPE_LABELS } from '../../types/Diary';
import type { FoodDto } from '../../types/Food';
import type { RecipeDto } from '../../types/Recipe';
import { RecipeCategory, RECIPE_CATEGORY_LABELS } from '../../types/Recipe';
import './AddFoodModal.scss';

type ItemType = 'food' | 'recipe';
type Source = 'recent' | 'frequent' | 'favorites';
type NutritionItem = FoodDto | RecipeDto;

interface StagedItem {
  item: NutritionItem;
  type: ItemType;
  quantityGrams: number;
}

interface AddFoodModalProps {
  open: boolean;
  onClose: () => void;
  date: string;
  defaultMealType?: MealType;
}

const mealLabel = (type: MealType, t: (key: string) => string) => {
  const keys: Partial<Record<MealType, string>> = {
    [MealType.Breakfast]: 'common:meals.breakfast', [MealType.Lunch]: 'common:meals.lunch',
    [MealType.Dinner]: 'common:meals.dinner', [MealType.Snack]: 'common:meals.snack',
    [MealType.Suhoor]: 'common:meals.suhoor', [MealType.Iftar]: 'common:meals.iftar',
    [MealType.PreWorkout]: 'common:meals.preWorkout', [MealType.PostWorkout]: 'common:meals.postWorkout',
  };
  return t(keys[type] ?? MEAL_TYPE_LABELS[type]);
};

const isFood = (item: NutritionItem): item is FoodDto => 'caloriesPer100g' in item;
const recipeCategory = (recipe: RecipeDto) => RECIPE_CATEGORY_LABELS[recipe.category as RecipeCategory] ?? String(recipe.category);

const AddFoodModal: React.FC<AddFoodModalProps> = ({ open, onClose, date, defaultMealType = MealType.Breakfast }) => {
  const { t } = useTranslation(['common', 'athlete']);
  const [itemType, setItemType] = useState<ItemType>('food');
  const [source, setSource] = useState<Source>('recent');
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [selected, setSelected] = useState<NutritionItem | null>(null);
  const [selectedType, setSelectedType] = useState<ItemType>('food');
  const [quantity, setQuantity] = useState(100);
  const [staged, setStaged] = useState<StagedItem[]>([]);

  // Local favorites cache for real-time toggling state in search
  const favoriteFoodsList = useGetFilteredNutritionItems('food', 'favorites', open);
  const favoriteRecipesList = useGetFilteredNutritionItems('recipe', 'favorites', open);

  const favoriteIds = useMemo(() => {
    const foods = new Set((favoriteFoodsList.data ?? []).map(f => f.id));
    const recipes = new Set((favoriteRecipesList.data ?? []).map(r => r.id));
    return { foods, recipes };
  }, [favoriteFoodsList.data, favoriteRecipesList.data]);

  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedSearch(search.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [search]);

  const filteredItems = useGetFilteredNutritionItems(itemType, source, open && !debouncedSearch);
  const foodSearch = useSearchFoods({ search: debouncedSearch, pageSize: 30 }, open && itemType === 'food' && !!debouncedSearch);
  const recipeSearch = useGetRecipes({ search: debouncedSearch, pageSize: 30 }, open && itemType === 'recipe' && !!debouncedSearch);
  const bulkLog = useBulkLogFood(date);
  const toggleFood = useToggleFavoriteFood();
  const toggleRecipe = useToggleFavoriteRecipe();

  const items = useMemo<NutritionItem[]>(() => {
    if (debouncedSearch) return itemType === 'food' ? (foodSearch.data?.items ?? []) : (recipeSearch.data?.items ?? []);
    return filteredItems.data ?? [];
  }, [debouncedSearch, itemType, foodSearch.data, recipeSearch.data, filteredItems.data]);
  const loading = debouncedSearch ? (itemType === 'food' ? foodSearch.isLoading : recipeSearch.isLoading) : filteredItems.isLoading;

  const preview = useMemo(() => {
    if (!selected) return null;
    if (isFood(selected)) return calcMacroPreview(selected, quantity);
    const totalWeight = selected.ingredients.reduce((sum, ingredient) => sum + ingredient.quantityGrams, 0) || 100;
    const multiplier = quantity / totalWeight;
    return {
      calories: selected.totalCalories * multiplier,
      protein: selected.totalProtein * multiplier,
      carbs: selected.totalCarbs * multiplier,
      fat: selected.totalFat * multiplier,
    };
  }, [selected, quantity]);

  const selectItem = (item: NutritionItem) => {
    setSelected(item);
    setSelectedType(isFood(item) ? 'food' : 'recipe');
    setQuantity(isFood(item) ? 100 : item.ingredients.reduce((sum, ingredient) => sum + ingredient.quantityGrams, 0) || 100);
  };

  const stageSelected = () => {
    if (!selected || quantity <= 0) return;
    setStaged(current => {
      const next = current.filter(entry => !(entry.type === selectedType && entry.item.id === selected.id));
      return [...next, { item: selected, type: selectedType, quantityGrams: quantity }];
    });
    setSelected(null);
  };

  const resetAndClose = () => {
    setItemType('food'); setSource('recent'); setSearch(''); setDebouncedSearch('');
    setSelected(null); setQuantity(100); setStaged([]);
    onClose();
  };

  const submit = () => {
    if (!staged.length) return;
    bulkLog.mutate({
      date,
      mealType: defaultMealType,
      items: staged.map(entry => ({
        foodId: entry.type === 'food' ? entry.item.id : null,
        recipeId: entry.type === 'recipe' ? entry.item.id : null,
        quantityGrams: entry.quantityGrams,
      })),
    }, { onSuccess: resetAndClose });
  };

  const stagedContent = (
    <div className="add-food-modal__staged-popover">
      {staged.length === 0 ? <span>{t('athlete:components.addFoodModal.noStaged', { defaultValue: 'No items staged yet.' })}</span> : staged.map(entry => (
        <div className="add-food-modal__staged-row" key={`${entry.type}-${entry.item.id}`}>
          <span>{entry.item.name} · {entry.quantityGrams}g</span>
          <button type="button" aria-label="Remove staged item" onClick={() => setStaged(current => current.filter(item => item !== entry))}>
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>
      ))}
    </div>
  );

  return (
    <Modal open={open} onCancel={resetAndClose} footer={null} closable={false} width={680} className="add-food-modal" destroyOnHidden>
      <div className="add-food-modal__screen">
        <header className="add-food-modal__header">
          <button type="button" className="add-food-modal__close" onClick={resetAndClose} aria-label="Close"><span className="material-symbols-outlined">close</span></button>
          <div>
            <span className="add-food-modal__eyebrow">{mealLabel(defaultMealType, t)}</span>
            <h2>{t('athlete:components.addFoodModal.prompt', { meal: mealLabel(defaultMealType, t).toLowerCase(), defaultValue: `What did you have for ${mealLabel(defaultMealType, t).toLowerCase()}?` })}</h2>
          </div>
        </header>

        <Input
          value={search}
          onChange={event => setSearch(event.target.value)}
          allowClear
          prefix={<span className="material-symbols-outlined">search</span>}
          placeholder={t('athlete:components.addFoodModal.searchPlaceholder', { defaultValue: 'Search foods and recipes' })}
          className="add-food-modal__search"
          size="large"
        />

        <div className="add-food-modal__filters">
          <Select value={itemType} onChange={value => { setItemType(value); setSearch(''); }} options={[
            { value: 'food', label: t('athlete:components.addFoodModal.tabFood') },
            { value: 'recipe', label: t('athlete:components.addFoodModal.tabRecipe') },
          ]} />
          <Select value={source} onChange={setSource} options={[
            { value: 'recent', label: t('athlete:components.addFoodModal.recent', { defaultValue: 'Recent' }) },
            { value: 'frequent', label: t('athlete:components.addFoodModal.frequent', { defaultValue: 'Frequent' }) },
            { value: 'favorites', label: t('athlete:components.addFoodModal.favorites', { defaultValue: 'Favorites' }) },
          ]} />
        </div>

        <main className="add-food-modal__results">
          {loading ? <div className="add-food-modal__loading"><Spin /> </div> : items.length === 0 ? (
            <Empty description={debouncedSearch ? t('athlete:components.addFoodModal.emptyRecipe') : t('athlete:components.addFoodModal.emptyFiltered', { defaultValue: 'No items here yet. Search to browse the library.' })} />
          ) : items.map(item => {
            const food = isFood(item);
            const favorite = food ? favoriteIds.foods.has(item.id) : favoriteIds.recipes.has(item.id);
            return <article className="add-food-modal__card" role="button" tabIndex={0} key={`${itemType}-${item.id}`} onClick={() => selectItem(item)} onKeyDown={event => event.key === 'Enter' && selectItem(item)}>
              <div className="add-food-modal__item-copy">
                <div className="add-food-modal__item-name">
                  {item.name}
                  {food && !item.isCustom && <span className="material-symbols-outlined add-food-modal__verified" title="Verified">verified</span>}
                  <button type="button" className={favorite ? 'is-favorite' : ''} aria-label="Toggle favorite" onClick={event => { event.stopPropagation(); food ? toggleFood.mutate(item.id) : toggleRecipe.mutate(item.id); }}>
                    <span className="material-symbols-outlined">{favorite ? 'star' : 'star_outline'}</span>
                  </button>
                </div>
                <span className="add-food-modal__subtitle">{food ? (item.category ? `${item.category} · 100g` : '100g') : `${recipeCategory(item)} · ${item.servings} ${t('athlete:components.recipeCard.servings')}`}</span>
                <span className="add-food-modal__calories">{Math.round(food ? item.caloriesPer100g : item.totalCalories)} {t('common:units.kcal')}</span>
              </div>
              <button type="button" className="add-food-modal__quick-add" aria-label="Choose quantity" onClick={event => { event.stopPropagation(); selectItem(item); }}><span className="material-symbols-outlined">add</span></button>
            </article>;
          })}
        </main>

        <footer className="add-food-modal__bottom-bar">
          <Popover content={stagedContent} trigger="click" placement="topLeft">
            <button type="button" className="add-food-modal__stage-count" aria-label="Review staged items">{staged.length}</button>
          </Popover>
          <Button type="primary" size="large" disabled={!staged.length} loading={bulkLog.isPending} onClick={submit}>{t('athlete:components.addFoodModal.done', { defaultValue: 'Done' })}</Button>
        </footer>
      </div>

      <Drawer open={!!selected} onClose={() => setSelected(null)} placement="bottom" height="auto" mask={false} rootClassName="add-food-modal__drawer-root" className="add-food-modal__drawer" title={selected?.name}>
        {selected && preview && <>
          <div className="add-food-modal__drawer-subtitle">{isFood(selected) ? (selected.category ? `${selected.category} · 100g` : '100g') : `${recipeCategory(selected)} · ${selected.servings} ${t('athlete:components.recipeCard.servings')}`}</div>
          <label className="add-food-modal__quantity-label">{t('athlete:components.addFoodModal.quantity')}</label>
          <InputNumber value={quantity} min={1} max={9999} addonAfter="g" size="large" onChange={value => setQuantity(value ?? 0)} className="add-food-modal__quantity" />
          <div className="add-food-modal__macro-preview">
            <span><b>{Math.round(preview.calories)}</b> kcal</span><span><b>{preview.protein.toFixed(1)}g</b> P</span><span><b>{preview.carbs.toFixed(1)}g</b> C</span><span><b>{preview.fat.toFixed(1)}g</b> F</span>
          </div>
          <Button type="primary" size="large" block disabled={quantity <= 0} onClick={stageSelected}>{t('athlete:components.addFoodModal.stage', { defaultValue: 'Stage item' })}</Button>
        </>}
      </Drawer>
    </Modal>
  );
};

export default AddFoodModal;
