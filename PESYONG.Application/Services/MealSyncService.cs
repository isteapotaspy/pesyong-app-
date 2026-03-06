using System;

namespace PESYONG.ApplicationLogic.Services
{
    public class MealSyncService
    {
        public event Action? MealsChanged;
        public void NotifyMealsChanged()
        {
            MealsChanged?.Invoke();
        }
    }
}