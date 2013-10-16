#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QString>
#include <QSettings>
#include <QTimer>
#include "qextserialport.h"


namespace Ui {
    class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = 0);
    ~MainWindow();

private:


private:
    QextSerialPort *port;
    int portIndex;
    QTimer delayTimer;

    // number of bytes per line
    static const int LINE_LENGTH;

private slots:
  //  void selectImage();
    void onReadyRead();
    void createBubbleLine();
    void selectSerialPort(int ind);
    void onChkEnAutoToggled(bool checked);
    void onFreqSliderMoved(int i);

private:
    Ui::MainWindow *ui;
};

#endif // MAINWINDOW_H
