function timestamper {
    $app = Get-Command $MyInvocation.InvocationName -CommandType Application | Select-Object -First 1
    & TimeStamper.exe $app.Source @args
}
