@echo off
cls
echo EXE split
..\bin\split binary data/sonic.exe newsplit/STG00.ini output/
..\bin\split binary data/sonic.exe newsplit/STG01.ini output/
..\bin\split binary data/sonic.exe newsplit/STG02.ini output/
..\bin\split binary data/sonic.exe newsplit/STG03.ini output/
..\bin\split binary data/sonic.exe newsplit/STG04.ini output/
..\bin\split binary data/sonic.exe newsplit/STG05.ini output/
..\bin\split binary data/sonic.exe newsplit/STG06.ini output/
..\bin\split binary data/sonic.exe newsplit/STG07.ini output/
..\bin\split binary data/sonic.exe newsplit/STG08.ini output/
..\bin\split binary data/sonic.exe newsplit/STG09.ini output/
..\bin\split binary data/sonic.exe newsplit/STG10.ini output/
..\bin\split binary data/sonic.exe newsplit/STG12.ini output/
..\bin\split binary data/sonic.exe newsplit/ADV00.ini output/
..\bin\split binary data/sonic.exe newsplit/ADV0100.ini output/
..\bin\split binary data/sonic.exe newsplit/ADV0130.ini output/
..\bin\split binary data/sonic.exe newsplit/ADV02.ini output/
..\bin\split binary data/sonic.exe newsplit/ADV03.ini output/
..\bin\split binary data/sonic.exe newsplit/ADVERTISE.ini output/
..\bin\split binary data/sonic.exe newsplit/Animals.ini output/
..\bin\split binary data/sonic.exe newsplit/B_CHAOS0.ini output/
..\bin\split binary data/sonic.exe newsplit/B_CHAOS2.ini output/
..\bin\split binary data/sonic.exe newsplit/B_CHAOS4.ini output/
..\bin\split binary data/sonic.exe newsplit/B_CHAOS6.ini output/
..\bin\split binary data/sonic.exe newsplit/B_CHAOS7.ini output/
..\bin\split binary data/sonic.exe newsplit/B_E101.ini output/
..\bin\split binary data/sonic.exe newsplit/B_E101_R.ini output/
..\bin\split binary data/sonic.exe newsplit/B_EGM1.ini output/
..\bin\split binary data/sonic.exe newsplit/B_EGM2.ini output/
..\bin\split binary data/sonic.exe newsplit/B_EGM3.ini output/
..\bin\split binary data/sonic.exe newsplit/B_ROBO.ini output/
..\bin\split binary data/sonic.exe newsplit/Chao.ini output/
..\bin\split binary data/sonic.exe newsplit/Characters.ini output/
..\bin\split binary data/sonic.exe newsplit/CommonObjects.ini output/
..\bin\split binary data/sonic.exe newsplit/Debug.ini output/
..\bin\split binary data/sonic.exe newsplit/Enemies.ini output/
..\bin\split binary data/sonic.exe newsplit/Event.ini output/
..\bin\split binary data/sonic.exe newsplit/Fish.ini output/
..\bin\split binary data/sonic.exe newsplit/MINICART.ini output/
..\bin\split binary data/sonic.exe newsplit/Mission.ini output/
..\bin\split binary data/sonic.exe newsplit/SBOARD.ini output/
..\bin\split binary data/sonic.exe newsplit/SHOOTING.ini output/
..\bin\split binary data/sonic.exe newsplit/Texlists.ini output/
..\bin\split binary data/sonic.exe newsplit/Sounds.ini output/
echo DLL split
..\bin\split binary data/system/CHRMODELS_orig.DLL newsplit/CHRMODELS.ini output/
..\bin\split binary data/system/BOSSCHAOS0MODELS.DLL newsplit/B_CHAOS0_DLL.ini output/
..\bin\split binary data/system/CHAOSTGGARDEN02MR_DAYTIME.DLL newsplit/chaostggarden02mr_daytime.ini output/
..\bin\split binary data/system/CHAOSTGGARDEN02MR_EVENING.DLL newsplit/chaostggarden02mr_evening.ini output/
..\bin\split binary data/system/CHAOSTGGARDEN02MR_NIGHT.DLL newsplit/chaostggarden02mr_night.ini output/
..\bin\split binary data/system/ADV00MODELS.DLL newsplit/ADV00_DLL.ini output/
..\bin\split binary data/system/ADV01MODELS.DLL newsplit/ADV0100_DLL.ini output/
..\bin\split binary data/system/ADV01CMODELS.DLL newsplit/ADV0130_DLL.ini output/
..\bin\split binary data/system/ADV02MODELS.DLL newsplit/ADV02_DLL.ini output/
..\bin\split binary data/system/ADV03MODELS.DLL newsplit/ADV03_DLL.ini output/
echo NB split
..\bin\split nb data/system/E101R_GC.NB output/ -ini newsplit/B_E101_R_NB.ini
..\bin\split nb data/system/EROBO_GC.NB output/ -ini newsplit/B_ROBO_NB.ini