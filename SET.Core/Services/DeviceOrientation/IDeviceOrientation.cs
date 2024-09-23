using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;
public interface IDeviceOrientation
{
    DeviceOrientationType GetOrientation();
}
