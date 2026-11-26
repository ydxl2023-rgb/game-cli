"""Convert the recovered separate music into a Unity-supported PCM file."""
from pathlib import Path
import sys,subprocess
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'client/Library/OpeningTools'))
import imageio_ffmpeg
subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-y','-i',str(root/'extracted-resources/assets/data/audios/music/start_game.0.m4a'),str(root/'client/Assets/Data/Widgets/Home/Opening/start_game.wav')],check=True)
