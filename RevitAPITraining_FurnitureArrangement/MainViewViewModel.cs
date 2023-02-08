using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITraining_FurnitureArrangement
{
    internal class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<FamilySymbol> FamilyList { get; } = new List<FamilySymbol>();
        public FamilySymbol SelectedFamily { get; set; }
        public List<Level> LevelList { get; } = new List<Level>();
        public Level SelectedLevel { get; set; }
        public DelegateCommand SetFamily { get; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            FamilyList = Utils.GetFamilyList(commandData);
            LevelList = Utils.GetLevels(commandData);
            SetFamily = new DelegateCommand(OnSetFamily);
        }

        private void OnSetFamily()
        {
            RaiseHideRequest();

            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            XYZ pickedPoint = null;
            try
            {
                pickedPoint = uidoc.Selection.PickPoint("Выберите точку");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
            }
            if (SelectedFamily == null ||
                SelectedLevel == null ||
                pickedPoint == null)
            {
                return;
            }
            Utils.CreateFamilyInstance(_commandData, SelectedFamily, pickedPoint, SelectedLevel);

            RaiseShowRequest();
        }

        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler CloseRequest;
        public void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

    }
    public class Utils
    {
        public static List<FamilySymbol> GetFamilyList(ExternalCommandData commandData)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            var familyList = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .ToList();
            return familyList;
        }
        public static List<Level> GetLevels(ExternalCommandData commandData)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();
            return levels;
        }
        public static FamilyInstance CreateFamilyInstance(ExternalCommandData commandData, FamilySymbol oFamSymb, XYZ InsertionPoint, Level oLevel)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            FamilyInstance familyInstance = null;

            using (var ts = new Transaction(doc, "Create family Instance"))
            {
                ts.Start();
                if (!oFamSymb.IsActive)
                {
                    oFamSymb.Activate();
                    doc.Regenerate();
                }
                familyInstance = doc.Create.NewFamilyInstance(
                    InsertionPoint,
                    oFamSymb,
                    oLevel,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                ts.Commit();
            }
            return familyInstance;
        }

    }
}
