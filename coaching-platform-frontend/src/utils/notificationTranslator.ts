import type { TFunction } from 'i18next';

/**
 * Translates notification messages from English to the selected language if needed.
 * This keeps the database messages in a clean, standard English format, preserving
 * client-side parsing logic, while showing fully translated Arabic/English text to users.
 */
export const translateNotificationMessage = (message: string, t: TFunction): string => {
  if (!message) return '';

  // 1. New workout program template assigned
  const workoutTemplateMatch = message.match(/^New workout program template assigned: (.*)$/i);
  if (workoutTemplateMatch) {
    return t('common:notifications.messages.newWorkoutTemplate', {
      templateName: workoutTemplateMatch[1],
      defaultValue: message
    });
  }

  // 2. Your coach left you a new feedback note
  const feedbackNoteMatch = message.match(/^Your coach left you a new feedback note: "([\s\S]*)"$/i);
  if (feedbackNoteMatch) {
    return t('common:notifications.messages.newFeedbackNote', {
      noteText: feedbackNoteMatch[1],
      defaultValue: message
    });
  }

  // 3. Daily targets updated: {calories} kcal, {protein}g P, {carbs}g C, {fat}g F, {water}L Water, {steps} Steps.
  const dailyTargetsMatch = message.match(
    /^Daily targets updated: ([\d.]+) kcal, ([\d.]+)g P, ([\d.]+)g C, ([\d.]+)g F, ([\d.]+)L Water, ([\d.]+) Steps\.$/i
  );
  if (dailyTargetsMatch) {
    return t('common:notifications.messages.dailyTargetsUpdated', {
      calories: dailyTargetsMatch[1],
      protein: dailyTargetsMatch[2],
      carbs: dailyTargetsMatch[3],
      fat: dailyTargetsMatch[4],
      water: dailyTargetsMatch[5],
      steps: dailyTargetsMatch[6],
      defaultValue: message
    });
  }

  // 4. Daily nutrition targets updated: {calories} kcal, {protein}g P, {carbs}g C, {fat}g F.
  const nutritionTargetsMatch = message.match(
    /^Daily nutrition targets updated: ([\d.]+) kcal, ([\d.]+)g P, ([\d.]+)g C, ([\d.]+)g F\.$/i
  );
  if (nutritionTargetsMatch) {
    return t('common:notifications.messages.dailyNutritionUpdated', {
      calories: nutritionTargetsMatch[1],
      protein: nutritionTargetsMatch[2],
      carbs: nutritionTargetsMatch[3],
      fat: nutritionTargetsMatch[4],
      defaultValue: message
    });
  }

  // 5. Daily activity targets updated: {water}L Water, {steps} Steps.
  const activityTargetsMatch = message.match(
    /^Daily activity targets updated: ([\d.]+)L Water, ([\d.]+) Steps\.$/i
  );
  if (activityTargetsMatch) {
    return t('common:notifications.messages.dailyActivityUpdated', {
      water: activityTargetsMatch[1],
      steps: activityTargetsMatch[2],
      defaultValue: message
    });
  }

  // 6. Daily targets updated.
  if (message.trim().toLowerCase() === 'daily targets updated.') {
    return t('common:notifications.messages.dailyTargetsUpdatedSimple', {
      defaultValue: message
    });
  }

  // 7. Check-in submitted
  const checkInSubmittedMatch = message.match(/^(.*) submitted their weekly check-in\.$/i);
  if (checkInSubmittedMatch) {
    return t('common:notifications.messages.checkInSubmitted', {
      athleteName: checkInSubmittedMatch[1],
      defaultValue: message
    });
  }

  // 8. Check-in updated
  const checkInUpdatedMatch = message.match(/^(.*) updated their weekly check-in\.$/i);
  if (checkInUpdatedMatch) {
    return t('common:notifications.messages.checkInUpdated', {
      athleteName: checkInUpdatedMatch[1],
      defaultValue: message
    });
  }

  // 9. Your coach reviewed your weekly check-in
  const checkInReviewedMatch = message.match(/^Your coach reviewed your weekly check-in: "([\s\S]*)"$/i);
  if (checkInReviewedMatch) {
    return t('common:notifications.messages.checkInReviewed', {
      notes: checkInReviewedMatch[1],
      defaultValue: message
    });
  }

  // 10. Athlete accepted invitation
  const invitationAcceptedMatch = message.match(/^Athlete (.*) has accepted your invitation\.$/i);
  if (invitationAcceptedMatch) {
    return t('common:notifications.messages.invitationAccepted', {
      athleteName: invitationAcceptedMatch[1],
      defaultValue: message
    });
  }

  // 11. Coach updated daily macro targets
  const macroTargetsUpdatedMatch = message.match(/^Coach (.*) updated your daily macro targets\.$/i);
  if (macroTargetsUpdatedMatch) {
    return t('common:notifications.messages.macroTargetsUpdated', {
      coachName: macroTargetsUpdatedMatch[1],
      defaultValue: message
    });
  }

  // Fallback to original message
  return message;
};
