#include "mainwindow.h"
#include "ui_mainwindow.h"
#include "qdebug.h"
//#include <stdint.h>
#include <qextserialenumerator.h>
#include <QMessageBox>
#include <QFileDialog>
#include <QMessageBox>
#include <QListWidgetItem>
#include <QColor>

const int MainWindow::LINE_LENGTH = 8;


MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    portIndex = -1;
    port = 0;
    ui->setupUi(this);
   // config = new QSettings("config.ini", QSettings::IniFormat);

     // fill all known ports into the combo-box
    QList<QextPortInfo> ports = QextSerialEnumerator::getPorts();
    for (int i=0; i<ports.size(); i++)
        ui->boxPort->addItem(ports.at(i).friendName,ports.at(i).physName);

    connect(&delayTimer,SIGNAL(timeout()),this,SLOT(createBubbleLine()));
    connect(ui->boxPort,SIGNAL(currentIndexChanged(int)), this, SLOT(selectSerialPort(int)));
    connect(ui->btnBubble, SIGNAL(clicked()), this, SLOT(btnBubbleClicked()));
    connect(ui->chkBoxEnAuto, SIGNAL(toggled(bool)), this, SLOT(onChkEnAutoToggled(bool)));
    connect(ui->sliderFreq, SIGNAL(sliderMoved(int)), this, SLOT(onFreqSliderMoved(int)));
    connect(ui->btnSelectFile, SIGNAL(clicked()), this, SLOT(selectImage()));

    // disable the graphical items
    //ui->listSerialLog->setDisabled(true);
    //ui->lineStatus->setDisabled(true);

    /*if (!config->contains("serial"))
    {
        // the config file contains no serial number, start with 0
        config->setValue("serial",QVariant(0));
        serial = 0;
    }
    else
    {
        bool ok;
        serial = config->value("serial").toInt(&ok);
        if (!ok)
        {
            qDebug("Can't parse serial number, exiting");
            exit(-1);
        }
    }
    ui->lineEditSerialNo->setText(QString().setNum(serial));

    ui->lineTestHex->setText(config->value("TestBitStreamHexFile","hwTestFw.hex").toString());
    ui->lineBootloaderHex->setText(config->value("BootloaderHexFile","hwTestFw.hex").toString());
    */
}


void MainWindow::selectImage()
{
    QFileDialog dialog(this,"select test bitstream hex-file", ".");
    dialog.setModal(true);
    dialog.setDefaultSuffix("png");
    dialog.exec();
    if (dialog.selectedFiles().length() != 1)
    {
        qDebug() << dialog.selectedFiles().length();
        QMessageBox::critical(this, "Error", "please select a hex file");
        return;
    }
    QString fn = dialog.selectedFiles().at(0);
    ui->lineEditImageFn->setText(fn);
    image = new QImage(fn);
    if (image->width() > LINE_LENGTH*8)
    {
        QMessageBox::critical(this, "Image Error", "The selected image is too wide");
        delete image;
        image = NULL;
    }
    ui->chkBoxEnAuto->setChecked(false);
    delayTimer.start();
    delayTimer.setInterval(ui->sliderFreq->value());
    rowCnt = 0;
}

MainWindow::~MainWindow()
{
    delete ui;
    //delete config;
}


void MainWindow::btnBubbleClicked()
{
    // create a bubble by sending the according data
    char data[LINE_LENGTH]; // = new uint8_t[LINE_LENGTH];

    // create a row of bubbles
    for (int i=0; i<LINE_LENGTH; i++)
        data[i] = 0xFF; // turn all valves off

    port->write(data, LINE_LENGTH);
}

// create a line of bubbles (in all channels)
void MainWindow::createBubbleLine()
{
    // create a bubble by sending the according data
    char data[LINE_LENGTH]; // = new uint8_t[LINE_LENGTH];


    for (int i=0; i<LINE_LENGTH; i++)
        data[i] = 0x00; // turn all valves off

    if (ui->chkBoxEnAuto->checkState())
    {
        // create a row of bubbles
        for (int i=0; i<LINE_LENGTH; i++)
            data[i] = 0xFF; // turn all valves off
    }
    else
    {
        if (image != NULL)
        {
            int x = 0;
            for (int i=0; i<LINE_LENGTH; i++)
            {
                for (int j=0; j<8; j++)
                {
                    if (x < image->width())
                    {
                        if (qGray(image->pixel(x,rowCnt)) < 128)
                                        data[i] |= (1<<j);
                    }
                    x++;
                }

            }
            qDebug() << "sending row " << rowCnt << ": ";
            for (int i=0; i<LINE_LENGTH; i++)
                qDebug() << "  " << (int)(data[i]) << "  ";
            // go to next line of image
            rowCnt++;
            if (rowCnt >= image->height())
                rowCnt = 0;
        }
        else
            qDebug() << "no image loaded";
    }

    port->write(data, LINE_LENGTH);

   //qDebug() << "created on line of bubbles";
}


// this slot is called when new data arrives on the serial port
void MainWindow::onReadyRead()
{
    QByteArray bytes;
    int a = port->bytesAvailable();
    bytes.resize(a);
    port->read(bytes.data(), bytes.size());
    //qDebug() << "bytes read:" << bytes.size();
    //qDebug() << "bytes:" << bytes;

    // we don't expect to receive anything -> ignore all data

}



// called when the user checks or unchecks the enable auto bubble checkbox
void MainWindow::onChkEnAutoToggled(bool checked)
{
    if (checked)
    {

        delayTimer.start();
        delayTimer.setInterval(ui->sliderFreq->value());
    }
    else
        delayTimer.stop();

}

// slider moved -> update timer interval and line edit
void MainWindow::onFreqSliderMoved(int i)
{
    ui->lineEditFreq->setText(QString::number(i));
    //if (ui->chkBoxEnAuto->isChecked())
        delayTimer.setInterval(i);
}

void MainWindow::selectSerialPort(int ind)
{
    QString portName = (ui->boxPort->itemData(ind)).toString();

    if (portIndex==ind)
        return;
    portIndex = ind;

    if (port)
    {
        port->close();
        delete port;
        port = 0;
    }

    port = new QextSerialPort(portName, QextSerialPort::EventDriven);
    port->setBaudRate(BAUD19200);
    port->setFlowControl(FLOW_OFF);
    port->setParity(PAR_NONE);
    port->setDataBits(DATA_8);
    port->setStopBits(STOP_1);

    if (port->open(QIODevice::ReadWrite) == true) {
        connect(port, SIGNAL(readyRead()), this, SLOT(onReadyRead()));
        //connect(port, SIGNAL(dsrChanged(bool)), this, SLOT(onDsrChanged(bool)));
        //if (!(port->lineStatus() & LS_DSR))
        //    qDebug() << "warning: device is not turned on";
        qDebug() << "listening for data on" << port->portName();
    }
    else {
        qDebug() << "device failed to open:" << port->errorString();
        return;
    }

   // ui->lineStatus->setText("RS232 connected");
    ui->btnBubble->setEnabled(true);    // enable button for bubble creation
    ui->chkBoxEnAuto->setEnabled(true);
    ui->btnSelectFile->setEnabled(true);
}
