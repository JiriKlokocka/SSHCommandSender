# UpgradeSwitches

Program odesílá 4 volitelné příkazy se 4 volitelnými parametry na libovolný počet ssh serverů

Definice všech parametrů je v souboru config.json

Každý `#sshVariable[X]#` je nahrazen příslušným `sshVariable[X]`.

Pokud neni sshPwd zadáno v JSONu, program vyzve k zadání ssh hesla. 

Username `sshUserName` musí být v JSONu

Program má režimy `test` a `run`, v testovacím režimu přidá před všechny příkazy `echo`

Všechny 4 příkazy a proměnné nemusí být nutně zadány

EXE je v adresáři `exe`, potřebuje .NET runtime 8

```
{
  "sshUserName": "sshtest",
  "sshPwd": "sshtest",
  "sshVariable1": "textOfVariable1",
  "sshVariable2": "textOfVariable2",
  "sshVariable3": "textOfVariable3",
  "sshVariable4": "",
  "sshCommand1": "Command1: #sshVariable1# #sshVariable2# #sshVariable3# #sshVariable4#",
  "sshCommand2": "Command2: #sshVariable1# #sshVariable2# #sshVariable3# #sshVariable4#",
  "sshCommand3": "Command3: #sshVariable1# #sshVariable2# #sshVariable3# #sshVariable4#",
  "sshCommand4": "",
  "sshIpList": [
    "10.40.1.162",
    "10.40.1.163",
    "10.40.1.162",
    "127.0.0.1",
    "127.0.0.1",
  ]
}
```
