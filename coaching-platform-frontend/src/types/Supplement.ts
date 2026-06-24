// ── Supplement Types ──────────────────────────────────────────────────────────

export enum SupplementType {
  Essential = 'Essential',
  Optional = 'Optional',
}

export interface SupplementDto {
  id: number;
  name: string;
  type: SupplementType;
  dosage: string | null;
  notes: string | null;
  isTakenToday: boolean;
  takenAt: string | null;
}

// ── Request Forms ─────────────────────────────────────────────────────────────

export interface ToggleSupplementForm {
  supplementScheduleId: number;
  date: string; // ISO YYYY-MM-DD
}
