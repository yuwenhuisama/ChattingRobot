// xfiat.cpp: 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"

#ifdef _WIN64
#pragma comment(lib,"./libs/msc_x64.lib") //x64
#else
#pragma comment(lib,"./libs/msc.lib") //x86
#endif

#define	BUFFER_SIZE	4096
#define FRAME_LEN	640 
#define HINTS_SIZE  100

extern "C" __declspec(dllexport) int __stdcall release(void* p) {
	free(p);
	return 0;
}

extern "C" __declspec(dllexport) int __stdcall run_iat(const char* audio_file, void** result)
{
	const char*		session_id = NULL;
	char			rec_result[BUFFER_SIZE] = { NULL };
	char			hints[HINTS_SIZE] = { NULL }; //hints为结束本次会话的原因描述，由用户自定义
	unsigned int	total_len = 0;
	int				aud_stat = MSP_AUDIO_SAMPLE_CONTINUE;		//音频状态
	int				ep_stat = MSP_EP_LOOKING_FOR_SPEECH;		//端点检测
	int				rec_stat = MSP_REC_STATUS_SUCCESS;			//识别状态
	int				errcode = MSP_SUCCESS;

	FILE*			f_pcm = NULL;
	char*			p_pcm = NULL;
	long			pcm_count = 0;
	long			pcm_size = 0;
	long			read_size = 0;

	char error_message_buffer[255];

	int func_result = 0;
	//void* result = NULL;

	const char* session_begin_params = "sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = gb2312";

	if (NULL == audio_file)
		goto iat_exit;

	f_pcm = fopen(audio_file, "rb");
	if (NULL == f_pcm)
	{
		sprintf(error_message_buffer, "\nopen [%s] failed! \n", audio_file);
		func_result = 1;
		goto iat_exit;
	}

	fseek(f_pcm, 0, SEEK_END);
	pcm_size = ftell(f_pcm); //获取音频文件大小 
	fseek(f_pcm, 0, SEEK_SET);

	p_pcm = (char *)malloc(pcm_size);
	if (NULL == p_pcm)
	{
		sprintf(error_message_buffer, "\nout of memory! \n");
		func_result = 1;
		goto iat_exit;
	}

	read_size = fread((void *)p_pcm, 1, pcm_size, f_pcm); //读取音频文件内容
	if (read_size != pcm_size)
	{
		sprintf(error_message_buffer, "\nread [%s] error!\n", audio_file);
		func_result = 1;
		goto iat_exit;
	}

	session_id = QISRSessionBegin(NULL, session_begin_params, &errcode); //听写不需要语法，第一个参数为NULL
	if (MSP_SUCCESS != errcode)
	{
		sprintf(error_message_buffer, "\nQISRSessionBegin failed! error code:%d\n", errcode);
		func_result = 1;
		goto iat_exit;
	}

	while (1)
	{
		unsigned int len = 10 * FRAME_LEN; // 每次写入200ms音频(16k，16bit)：1帧音频20ms，10帧=200ms。16k采样率的16位音频，一帧的大小为640Byte
		int ret = 0;

		if (pcm_size < 2 * len)
			len = pcm_size;
		if (len <= 0)
			break;

		aud_stat = MSP_AUDIO_SAMPLE_CONTINUE;
		if (0 == pcm_count)
			aud_stat = MSP_AUDIO_SAMPLE_FIRST;

		ret = QISRAudioWrite(session_id, (const void *)&p_pcm[pcm_count], len, aud_stat, &ep_stat, &rec_stat);
		if (MSP_SUCCESS != ret)
		{
			sprintf(error_message_buffer, "\nQISRAudioWrite failed! error code:%d\n", ret);
			func_result = 1;
			goto iat_exit;
		}

		pcm_count += (long)len;
		pcm_size -= (long)len;

		if (MSP_REC_STATUS_SUCCESS == rec_stat) //已经有部分听写结果
		{
			const char *rslt = QISRGetResult(session_id, &rec_stat, 0, &errcode);
			if (MSP_SUCCESS != errcode)
			{
				sprintf(error_message_buffer, "\nQISRGetResult failed! error code: %d\n", errcode);
				goto iat_exit;
			}
			if (NULL != rslt)
			{
				unsigned int rslt_len = strlen(rslt);
				total_len += rslt_len;
				if (total_len >= BUFFER_SIZE)
				{
					sprintf(error_message_buffer, "\nno enough buffer for rec_result !\n");
					func_result = 1;
					goto iat_exit;
				}
				strncat(rec_result, rslt, rslt_len);
			}
		}

		if (MSP_EP_AFTER_SPEECH == ep_stat)
			break;
		//Sleep(200); //模拟人说话时间间隙。200ms对应10帧的音频
	}
	errcode = QISRAudioWrite(session_id, NULL, 0, MSP_AUDIO_SAMPLE_LAST, &ep_stat, &rec_stat);
	if (MSP_SUCCESS != errcode)
	{
		sprintf(error_message_buffer, "\nQISRAudioWrite failed! error code:%d \n", errcode);
		func_result = 1;
		goto iat_exit;
	}

	while (MSP_REC_STATUS_COMPLETE != rec_stat)
	{
		const char *rslt = QISRGetResult(session_id, &rec_stat, 0, &errcode);
		if (MSP_SUCCESS != errcode)
		{
			sprintf(error_message_buffer, "\nQISRGetResult failed, error code: %d\n", errcode);
			func_result = 1;
			goto iat_exit;
		}
		if (NULL != rslt)
		{
			unsigned int rslt_len = strlen(rslt);
			total_len += rslt_len;
			if (total_len >= BUFFER_SIZE)
			{
				sprintf(error_message_buffer, "\nno enough buffer for rec_result !\n");
				func_result = 1;
				goto iat_exit;
			}
			strncat(rec_result, rslt, rslt_len);
		}
		Sleep(150); //防止频繁占用CPU
	}

iat_exit:
	if (NULL != f_pcm)
	{
		fclose(f_pcm);
		f_pcm = NULL;
	}
	if (NULL != p_pcm)
	{
		free(p_pcm);
		p_pcm = NULL;
	}

	QISRSessionEnd(session_id, hints);

	// success
	if (func_result == 0) {
		*result = malloc(sizeof(char) * total_len + 1);
		memcpy(*result, rec_result, sizeof(char) * total_len);
	}
	else {
		*result = malloc(sizeof(char) * strlen(error_message_buffer) + 1);
		memcpy(*result, error_message_buffer, sizeof(char) * strlen(error_message_buffer));
	}

	return func_result;
}

extern "C" __declspec(dllexport) int __stdcall log_in() {
	int			ret = MSP_SUCCESS;
	int			upload_on = 1; //是否上传用户词表
	const char* login_params = "appid = 5a1464ce, work_dir = ."; // 登录参数，appid与msc库绑定,请勿随意改动

																 /*
																 * sub:				请求业务类型
																 * domain:			领域
																 * language:			语言
																 * accent:			方言
																 * sample_rate:		音频采样率
																 * result_type:		识别结果格式
																 * result_encoding:	结果编码格式
																 *
																 */

	/* 用户登录 */
	return MSPLogin(NULL, NULL, login_params); //第一个参数是用户名，第二个参数是密码，均传NULL即可，第三个参数是登录参数	
}

extern "C" __declspec(dllexport) int __stdcall log_out() {
	return MSPLogout();
}