set key=customurl 
reg add HKCR\%key% /ve /d "URL:Description" 
reg add HKCR\%key% /v "URL Protocol" /d "" 
reg add HKCR\%key%\shell 
reg add HKCR\%key%\shell\open 
reg add HKCR\%key%\shell\open\command /ve /d ""C:\\Program Files\GhostWriter_x64\\ghostwriter.exe" ""%%1"""
pause