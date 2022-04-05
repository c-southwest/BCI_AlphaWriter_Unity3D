param(
    # Parameter help description
    [Parameter()]
    [string]
    $message
)
Add-Type -AssemblyName System.Speech
$Speech = New-Object System.Speech.Synthesis.SpeechSynthesizer
$Speech.Speak($message)