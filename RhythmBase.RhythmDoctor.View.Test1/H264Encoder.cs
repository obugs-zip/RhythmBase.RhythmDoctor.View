using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhythmBase.RhythmDoctor.View.Test1
{
    //public record struct AudioSource(string FilePath, double StartTime, double Volume = 1);
    //unsafe class AudioMixer
    //{
    //    private const int SampleRate = 44100;
    //    private const int Channels = 2;

    //    private List<float> _mixedPcm = [];

    //    public void AddSource(AudioSource source)
    //    {

    //    }
    //}
    unsafe internal class H264Encoder : IDisposable
    {
        private AVCodecContext* _codecCtx;
        private AVFormatContext* _fmtCtx;
        private AVStream* _stream;
        private AVFrame* _frame;
        private AVPacket* _packet;
        private SwsContext* _swsCtx;

        private int _width, _height, _fps;
        private long _pts = 0;
        private readonly string _outputPath;

        public H264Encoder(string outputPath, int width, int height, int fps)
        {
            _outputPath = outputPath;
            _width = width;
            _height = height;
            _fps = fps;

            ffmpeg.avformat_network_init();

            AVFormatContext* fmtCtx = null;
            ffmpeg.avformat_alloc_output_context2(&fmtCtx, null, null, outputPath);
            _fmtCtx = fmtCtx;

            var codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            _codecCtx = ffmpeg.avcodec_alloc_context3(codec);

            _codecCtx->width = width;
            _codecCtx->height = height;
            _codecCtx->time_base = new AVRational { num = 1, den = fps };
            _codecCtx->framerate = new AVRational { num = fps, den = 1 };
            _codecCtx->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;

            _codecCtx->bit_rate = 4_000_000;
            _codecCtx->gop_size = 30;

            ffmpeg.avcodec_open2(_codecCtx, codec, null);

            _stream = ffmpeg.avformat_new_stream(_fmtCtx, null);
            ffmpeg.avcodec_parameters_from_context(_stream->codecpar, _codecCtx);
            _stream->time_base = _codecCtx->time_base;

            ffmpeg.avio_open(&_fmtCtx->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE);
            ffmpeg.avformat_write_header(_fmtCtx, null);

            _frame = ffmpeg.av_frame_alloc();
            _frame->format = (int)_codecCtx->pix_fmt;
            _frame->width = _codecCtx->width;
            _frame->height = _codecCtx->height;
            ffmpeg.av_frame_get_buffer(_frame, 32);

            _packet = ffmpeg.av_packet_alloc();

            _swsCtx = ffmpeg.sws_getContext(width, height, AVPixelFormat.AV_PIX_FMT_BGRA,
                                            width, height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                                            2, null, null, null);

        }
        public void WriteFrame(nint bgraPtr, int srcLineSize)
        {
            if (bgraPtr == nint.Zero)
                return;

            byte* srcData = (byte*)bgraPtr;
            byte*[] srcDataArray = [srcData];
            int[] srcLineSizes = [srcLineSize];
            ffmpeg.sws_scale(_swsCtx, srcDataArray, srcLineSizes, 0, _height, _frame->data, _frame->linesize);
            
            _frame->pts = _pts++;
            ffmpeg.avcodec_send_frame(_codecCtx, _frame);
            while (ffmpeg.avcodec_receive_packet(_codecCtx, _packet) == 0)
            {
                _packet->stream_index = _stream->index;
                ffmpeg.av_packet_rescale_ts(_packet, _codecCtx->time_base, _stream->time_base);

                if (_packet->dts == ffmpeg.AV_NOPTS_VALUE)
                    _packet->dts = _packet->pts;

                ffmpeg.av_interleaved_write_frame(_fmtCtx, _packet);
                ffmpeg.av_packet_unref(_packet);
            }
        }
        public void Dispose()
        {
            ffmpeg.avcodec_send_frame(_codecCtx, null);
            while (ffmpeg.avcodec_receive_packet(_codecCtx, _packet) == 0)
            {
                _packet->stream_index = _stream->index;
                ffmpeg.av_packet_rescale_ts(_packet, _codecCtx->time_base, _stream->time_base);
                if(_packet->dts == ffmpeg.AV_NOPTS_VALUE)
                    _packet->dts = _packet->pts;
                ffmpeg.av_interleaved_write_frame(_fmtCtx, _packet);
                ffmpeg.av_packet_unref(_packet);
            }
            ffmpeg.av_write_trailer(_fmtCtx);

            ffmpeg.sws_freeContext(_swsCtx);
            fixed (AVFrame** framePtr = &_frame)
                ffmpeg.av_frame_free(framePtr);
            fixed (AVPacket** packetPtr = &_packet)
                ffmpeg.av_packet_free(packetPtr);
            fixed (AVCodecContext** codecCtxPtr = &_codecCtx)
                ffmpeg.avcodec_free_context(codecCtxPtr);
            ffmpeg.avio_closep(&_fmtCtx->pb);
            ffmpeg.avformat_free_context(_fmtCtx);
        }
    }
}
