powershell -command if ((Get-ItemProperty %1).lastwritetime -gt (Get-ItemProperty %2).lastwritetime) { ^& 'c:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x64\fxc.exe' /O3 /Fc /T fx_5_0 /Qstrip_reflect /Qstrip_debug /Qstrip_priv /Fo %2 %1; }
