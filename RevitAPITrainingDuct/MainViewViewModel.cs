using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace RevitAPITrainingDuct
{
    internal class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> SupplyAirs { get; } = new List<DuctType>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctCenter { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public DuctType SelectedSupplyAir { get; set; }
        public Level SelectedLevel { get; set; }
        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            SupplyAirs = DuctsUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctCenter = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedLevel == null || SelectedSupplyAir == null)
                return;

            var curves = new List<Curve>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;


                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];
                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                foreach (var curve in curves)
                {

                    Duct.Create(doc, curves, SelectedLevel.Id, SelectedSupplyAir.Id, UnitUtils.ConvertToInternalUnits(DuctCenter, UnitTypeId.Millimeters), 0, false, false);
                }
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
