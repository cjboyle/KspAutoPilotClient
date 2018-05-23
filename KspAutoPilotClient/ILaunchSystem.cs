using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public interface ILaunchSystem
    {
        float TargetApoapsis { get; set; }
        float TargetPeriapsis { get; set; }
        void Initialize();
        void Execute(bool countdown = true);
        Task ExecuteAsync(bool countdown = true); 
    }
    
    public interface ISingleStageLauncher : ILaunchSystem
    {
        Vehicle Launcher { get; set; }
    }

    public interface ITwoStageLauncher : ILaunchSystem
    {
        Vehicle Stage1 { get; set; }
        Vehicle Stage2 { get; set; }
    }

    public interface IThreeStageLauncher : ILaunchSystem
    {
        Vehicle Stage1 { get; set; }
        Vehicle Stage2 { get; set; }
        Vehicle Stage3 { get; set; }
    }

    public interface IPayload
    {

    }

    public interface ILandable
    {
        double TimeToSuicideBurn { get; }
    }
}
