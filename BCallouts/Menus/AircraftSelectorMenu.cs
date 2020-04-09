using RAGENativeUI;
using Rage;
using RAGENativeUI.Elements;

namespace BCallouts.Menus
{
    class AircraftSelectorMenu
    {

        private MenuPool MPool;

        private UIMenu MainMenu;
        private UIMenuItem GoToCarrierItem;
        private UIMenuItem NavigateToPlaneSelectionItem;

        private UIMenu PlaneSelectionMenu;
        private UIMenuListItem ModelList;
        private UIMenuItem SpawnPlaneItem;

        public AircraftSelectorMenu() {
            MPool = new MenuPool
            {
                (MainMenu = new UIMenu("Aircraft Selector", "Select an action")),
                (PlaneSelectionMenu = new UIMenu("Aircraft Selector", "Select an aircraft model") { ParentMenu = MainMenu })
            };
            MainMenu.Visible = false;
            PlaneSelectionMenu.Visible = false;
            MainMenu.AddItem(GoToCarrierItem = new UIMenuItem("Go to Carrier", "Travel to the Aircraft Carrier"));
            MainMenu.AddItem(NavigateToPlaneSelectionItem = new UIMenuItem("Take an Aircraft", "Navigate to the Aircraft Selection menu"));
            MainMenu.BindMenuToItem(PlaneSelectionMenu, NavigateToPlaneSelectionItem);


            PlaneSelectionMenu.AddItem(ModelList = new UIMenuListItem("Model", "Lets you select the aircraft model you want", AircraftManager.AircraftModels));
            PlaneSelectionMenu.AddItem(SpawnPlaneItem = new UIMenuItem("Start Flight", "Start a flight with the aircraft you selected"));

            GoToCarrierItem.Activated += GoToCarrier;
            SpawnPlaneItem.Activated += SpawnPlane;
            MPool.RefreshIndex();
        }

        public void Process() {
            MPool.ProcessMenus();
        }

        public void OpenMenu() {
            MainMenu.Visible = true;
        }

        public void CloseMenu() {
            MainMenu.Visible = false;
            PlaneSelectionMenu.Visible = false;
        }

        private void SpawnPlane(UIMenu sender, UIMenuItem selectedItem) {
            sender.Visible = false;
            AircraftManager.TakePlane((Model)ModelList.SelectedItem.Value);
        }

        private void GoToCarrier(UIMenu sender, UIMenuItem selectedItem) {
            sender.Visible = false;
            AircraftManager.TravelToCarrier();
        }
    }
}
