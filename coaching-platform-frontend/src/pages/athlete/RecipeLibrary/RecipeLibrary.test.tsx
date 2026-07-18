import type { ReactNode } from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MealType } from '../../../types/Diary';
import RecipeLibrary from './RecipeLibrary';

const mutateAsync = vi.fn();

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, values?: { name?: string }) => values?.name ? `${key}:${values.name}` : key,
  }),
}));

vi.mock('../../../contexts/LanguageContext', () => ({
  useLanguage: () => ({ language: 'ar' }),
}));

vi.mock('../../../hooks/useRecipes/useRecipes', () => ({
  useGetRecipes: () => ({
    data: {
      items: [{ id: 17, name: 'Coach Recipe', nameAr: 'وصفة المدرب' }],
    },
    isLoading: false,
  }),
  useQuickAddRecipe: () => ({ mutateAsync, isPending: false }),
}));

vi.mock('../../../components/RecipeCard/RecipeCard', () => ({
  default: ({ recipe, onQuickAdd }: { recipe: { id: number; name: string }; onQuickAdd: (recipe: unknown) => void }) => (
    <button onClick={() => onQuickAdd(recipe)}>Quick add {recipe.id}</button>
  ),
}));

vi.mock('../../../components/CreateRecipeModal/CreateRecipeModal', () => ({
  default: () => null,
}));

vi.mock('antd', () => {
  const RadioGroup = ({ value, onChange, children }: {
    value: number;
    onChange: (event: { target: { value: number } }) => void;
    children: ReactNode;
  }) => <div onChange={(event) => onChange({ target: { value: Number((event.target as HTMLInputElement).value) } })}>{children}</div>;
  const RadioButton = ({ value, children }: { value: number; children: ReactNode }) => (
    <label><input type="radio" name="meal" value={value} />{children}</label>
  );

  return {
    Tabs: ({ items }: { items: Array<{ key: string; children: ReactNode }> }) => <>{items[0]?.children}</>,
    Skeleton: () => null,
    Empty: Object.assign(() => null, { PRESENTED_IMAGE_SIMPLE: 'simple' }),
    Modal: ({ open, title, okText, onOk, children }: {
      open: boolean;
      title: ReactNode;
      okText: ReactNode;
      onOk: () => void;
      children: ReactNode;
    }) => open ? <div role="dialog"><h2>{title}</h2>{children}<button onClick={onOk}>{okText}</button></div> : null,
    Radio: Object.assign(() => null, { Group: RadioGroup, Button: RadioButton }),
  };
});

describe('RecipeLibrary meal selection', () => {
  beforeEach(() => {
    mutateAsync.mockReset().mockResolvedValue(undefined);
  });

  it('asks for a meal and adds the selected recipe to dinner', async () => {
    render(<RecipeLibrary />);

    fireEvent.click(screen.getByRole('button', { name: 'Quick add 17' }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('athlete:recipeLibrary.chooseMealDescription:وصفة المدرب')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('radio', { name: 'common:meals.dinner' }));
    fireEvent.click(screen.getByRole('button', { name: 'athlete:recipeLibrary.addToDiary' }));

    await waitFor(() => expect(mutateAsync).toHaveBeenCalledWith({
      id: 17,
      mealType: MealType.Dinner,
    }));
  });
});
