# SOOP Extension for Unity

Unity용 SOOP(숲) 라이브 스트리밍 플랫폼 API 라이브러리입니다.

## 종속성
이 패키지는 다음 종속성을 자동으로 설치합니다:
- **NativeWebSocket**: 웹소켓 라이브러리

자동 설치에 실패하면 수동으로 설치하세요:
1. 패키지 관리자 열기
2. '+' > git URL에서 패키지 추가하기를 클릭합니다
3. 입력: 'https://github.com/endel/NativeWebSocket.git#upm'

## ✨ 주요 기능

- **인증 API**: 로그인 및 쿠키 관리
- **라이브 API**: 스트림 정보 조회
- **채널 API**: 스트리머 정보 조회
- **실시간 채팅**: WebSocket 기반 채팅 수신
- **고성능**: NativeWebSocket 사용

## 📋 지원하는 이벤트

- **CONNECT**: 채팅 서버 연결
- **CHAT**: 일반 채팅 메시지
- **TEXT_DONATION**: 텍스트 후원
- **VIDEO_DONATION**: 영상 후원
- **SUBSCRIBE**: 구독
- **EMOTICON**: 이모티콘
- **NOTIFICATION**: 공지사항
- **VIEWER**: 입장/퇴장
- **DISCONNECT**: 연결 해제

## ⚙️ 설정

Unity 메뉴에서 **Edit > Project Settings > SOOP Extension**에서 설정 가능:
- API Base URLs
- User Agent
- 기타 옵션들

## 📝 예제

`Samples~/BasicUsage/` 폴더에서 다음 예제들을 확인하세요:
- **SoopChatExample**: 실시간 채팅 수신 예제
- **SoopAPIExample**: API 호출 예제

## 🔧 요구사항

- Unity 2022.3 이상
- NativeWebSocket 패키지

## 📄 라이센스

MIT License