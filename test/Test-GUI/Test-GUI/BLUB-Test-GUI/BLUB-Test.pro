QT       += core gui

TARGET = BLUB-Test
TEMPLATE = app

DEPENDPATH += .
INCLUDEPATH  += qextserialport
#QMAKE_LIBDIR += qextserialport/build

SOURCES += main.cpp\
        mainwindow.cpp

HEADERS  += mainwindow.h

FORMS    += mainwindow.ui

#CONFIG(debug, debug|release):LIBS += -lqextserialportd
#else:LIBS += -L/home/lukas/e-/hertz10/SWARM/V2.0/test-station/gui/swarm-test-station-gui/qextserialport/build -lqextserialport
#win32:LIBS += -lsetupapi

HEADERS                += qextserialport/qextserialport.h \
                          qextserialport/qextserialenumerator.h \
                          qextserialport/qextserialport_global.h
SOURCES                += qextserialport/qextserialport.cpp

unix:SOURCES           += qextserialport/posix_qextserialport.cpp
unix:!macx:SOURCES     += qextserialport/qextserialenumerator_unix.cpp
macx {
  SOURCES          += qextserialport/qextserialenumerator_osx.cpp
  LIBS             += -framework IOKit -framework CoreFoundation
}

win32 {
  SOURCES          += qextserialport/win_qextserialport.cpp qextserialport/qextserialenumerator_win.cpp
  DEFINES          += WINVER=0x0501 # needed for mingw to pull in appropriate dbt business...probably a better way to do this
  LIBS             += -lsetupapi
}





