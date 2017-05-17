Import-Module WebAdministration

# Terminate app after 4h without any request
Set-ItemProperty "IIS:\AppPools\DefaultAppPool" processModel.idleTimeout "04:00"

# Recycle/restart app every day at 3am
Set-ItemProperty "IIS:\AppPools\DefaultAppPool" recycling.periodicRestart.time "0"
Set-ItemProperty "IIS:\AppPools\DefaultAppPool" recycling.periodicRestart.schedule @{value="03:00"}
